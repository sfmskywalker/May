using Elsa.Common.Features;
using Elsa.Extensions;
using Elsa.Features.Abstractions;
using Elsa.Features.Attributes;
using Elsa.Features.Services;
using Elsa.Http.ContentWriters;
using Elsa.Http.Contracts;
using Elsa.Http.DownloadableProviders;
using Elsa.Http.Handlers;
using Elsa.Http.HostedServices;
using Elsa.Http.Models;
using Elsa.Http.Options;
using Elsa.Http.Parsers;
using Elsa.Http.PortResolvers;
using Elsa.Http.Providers;
using Elsa.Http.Selectors;
using Elsa.Http.Services;
using Elsa.JavaScript.Features;
using Elsa.Liquid.Features;
using Elsa.Workflows.Core.Contracts;
using Elsa.Workflows.Management.Requests;
using Elsa.Workflows.Management.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Elsa.Http.Features;

/// <summary>
/// Installs services related to HTTP services and activities.
/// </summary>
[DependsOn(typeof(MemoryCacheFeature))]
[DependsOn(typeof(LiquidFeature))]
[DependsOn(typeof(JavaScriptFeature))]
public class HttpFeature : FeatureBase
{
    /// <inheritdoc />
    public HttpFeature(IModule module) : base(module)
    {
    }

    /// <summary>
    /// A delegate to configure <see cref="HttpActivityOptions"/>.
    /// </summary>
    public Action<HttpActivityOptions>? ConfigureHttpOptions { get; set; }

    /// <summary>
    /// A delegate that is invoked when authorizing an inbound HTTP request.
    /// </summary>
    public Func<IServiceProvider, IHttpEndpointAuthorizationHandler> HttpEndpointAuthorizationHandler { get; set; } = sp => sp.GetRequiredService<AllowAnonymousHttpEndpointAuthorizationHandler>();

    /// <summary>
    /// A delegate that is invoked when an HTTP workflow faults. 
    /// </summary>
    public Func<IServiceProvider, IHttpEndpointWorkflowFaultHandler> HttpEndpointWorkflowFaultHandler { get; set; } = sp => sp.GetRequiredService<DefaultHttpEndpointWorkflowFaultHandler>();

    /// <summary>
    /// A delegate to configure the <see cref="HttpClient"/> used when by the <see cref="FlowSendHttpRequest"/> activity.
    /// </summary>
    public Action<IServiceProvider, HttpClient> HttpClient { get; set; } = (_, _) => { };

    /// <summary>
    /// A delegate to configure the <see cref="HttpClientBuilder"/> for <see cref="HttpClient"/>.
    /// </summary>
    public Action<IHttpClientBuilder> HttpClientBuilder { get; set; } = _ => { };

    /// <summary>
    /// A list of <see cref="IHttpCorrelationIdSelector"/> types to register with the service collection.
    /// </summary>
    public ICollection<Type> HttpCorrelationIdSelectorTypes { get; } = new List<Type>
    {
        typeof(HeaderHttpCorrelationIdSelector), 
        typeof(QueryStringHttpCorrelationIdSelector)
    };

    /// <summary>
    /// A list of <see cref="IHttpWorkflowInstanceIdSelector"/> types to register with the service collection.
    /// </summary>
    public ICollection<Type> HttpWorkflowInstanceIdSelectorTypes { get; } = new List<Type>
    {
        typeof(HeaderHttpWorkflowInstanceIdSelector), 
        typeof(QueryStringHttpWorkflowInstanceIdSelector)
    };

    /// <inheritdoc />
    public override void Configure()
    {
        Module.UseWorkflowManagement(management =>
        {
            management.AddVariableTypes(new[]
            {
                typeof(RouteData),
                typeof(HttpRequest),
                typeof(HttpResponse),
                typeof(HttpResponseMessage),
                typeof(HttpRequestHeaders)
            }, "HTTP");

            management.AddActivitiesFrom<HttpFeature>();
        });

        Services.AddRequestHandler<ValidateWorkflowRequestHandler, ValidateWorkflowRequest, ValidateWorkflowResponse>();
    }

    /// <inheritdoc />
    public override void ConfigureHostedServices()
    {
        ConfigureHostedService<UpdateRouteTableHostedService>();
    }

    /// <inheritdoc />
    public override void Apply()
    {
        var configureOptions = ConfigureHttpOptions ?? (options =>
        {
            options.BasePath = "/workflows";
            options.BaseUrl = new Uri("http://localhost");
        });

        Services.Configure(configureOptions);

        var httpClientBuilder = Services.AddHttpClient<SendHttpRequestBase>(HttpClient);
        HttpClientBuilder(httpClientBuilder);

        Services
            .AddSingleton<IRouteMatcher, RouteMatcher>()
            .AddSingleton<IRouteTable, RouteTable>()
            .AddSingleton<IAbsoluteUrlProvider, DefaultAbsoluteUrlProvider>()
            .AddSingleton<IHttpBookmarkProcessor, HttpBookmarkProcessor>()
            .AddSingleton<IRouteTableUpdater, DefaultRouteTableUpdater>()
            .AddNotificationHandlersFrom<UpdateRouteTable>()
            .AddHttpContextAccessor()

            // Content parsers.
            .AddSingleton<IHttpContentParser, StringHttpContentParser>()
            .AddSingleton<IHttpContentParser, JsonHttpContentParser>()
            .AddSingleton<IHttpContentParser, XmlHttpContentParser>()

            // HTTP content factories.
            .AddSingleton<IHttpContentFactory, TextContentFactory>()
            .AddSingleton<IHttpContentFactory, JsonContentFactory>()
            .AddSingleton<IHttpContentFactory, XmlContentFactory>()
            .AddSingleton<IHttpContentFactory, FormUrlEncodedHttpContentFactory>()

            // Activity property options providers.
            .AddSingleton<IActivityPropertyOptionsProvider, WriteHttpResponseContentTypeOptionsProvider>()

            // Port resolvers.
            .AddSingleton<IActivityPortResolver, SendHttpRequestActivityPortResolver>()

            // HTTP endpoint handlers.
            .AddSingleton<AuthenticationBasedHttpEndpointAuthorizationHandler>()
            .AddSingleton<AllowAnonymousHttpEndpointAuthorizationHandler>()
            .AddSingleton<DefaultHttpEndpointWorkflowFaultHandler>()
            .AddSingleton(HttpEndpointWorkflowFaultHandler)
            .AddSingleton(HttpEndpointAuthorizationHandler)
            
            // File related services.
            .AddSingleton<IDownloadableManager, DefaultDownloadableManager>()
            .AddSingleton<IDownloadableProvider, BinaryDownloadableProvider>()
            .AddSingleton<IDownloadableProvider, DownloadableDownloadableProvider>()
            .AddSingleton<IDownloadableProvider, MultiDownloadableProvider>()
            .AddSingleton<IDownloadableProvider, StreamDownloadableProvider>()
            .AddSingleton<IDownloadableProvider, UrlDownloadableProvider>()

            // Add mediator handlers.
            .AddNotificationHandlersFrom<HttpFeature>()

            // AuthenticationBasedHttpEndpointAuthorizationHandler requires Authorization services.
            // We could consider creating a separate module for installing authorization services.
            .AddAuthorization();

        // HTTP clients.
        Services.AddHttpClient<IFileDownloader, HttpClientFileDownloader>();
        
        // Add selectors.
        foreach (var httpCorrelationIdSelectorType in HttpCorrelationIdSelectorTypes) 
            Services.AddSingleton(typeof(IHttpCorrelationIdSelector), httpCorrelationIdSelectorType);
        
        foreach (var httpWorkflowInstanceIdSelectorType in HttpWorkflowInstanceIdSelectorTypes)
            Services.AddSingleton(typeof(IHttpWorkflowInstanceIdSelector), httpWorkflowInstanceIdSelectorType);
    }
}