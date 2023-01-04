using Elsa.Extensions;
using Elsa.Features.Abstractions;
using Elsa.Features.Services;
using Elsa.Webhooks.Implementations;
using Elsa.Webhooks.Models;
using Elsa.Webhooks.Options;
using Elsa.Webhooks.Services;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Elsa.Webhooks.Features;

/// <summary>
/// Installs and configures services that let the user register webhook endpoints.
/// </summary>
public class WebhooksFeature : FeatureBase
{
    /// <inheritdoc />
    public WebhooksFeature(IModule module) : base(module)
    {
    }

    /// <summary>
    /// A delegate that resolves the <see cref="IWebhookDispatcher"/> to use.
    /// </summary>
    public Func<IServiceProvider, IWebhookDispatcher> WebhookDispatcher { get; set; } = sp => sp.GetRequiredService<BackgroundWebhookDispatcher>();

    /// <summary>
    /// A delegate that is invoked when configuring <see cref="WebhookOptions"/>.
    /// </summary>
    public Action<WebhookOptions> ConfigureWebhookOptions { get; set; } = _ => { };

    /// <summary>
    /// A delegate to configure the <see cref="HttpClient"/> used when invoking webhook endpoints.
    /// </summary>
    public Action<IServiceProvider, HttpClient> ConfigureHttpClient { get; set; } = (_, _) => { };

    /// <summary>
    /// A delegate to configure the <see cref="IHttpClientBuilder"/>. For example, to configure Polly policies.
    /// </summary>
    public Action<IHttpClientBuilder> ConfigureHttpClientBuilder { get; set; } = builder => builder.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(retry)));

    /// <summary>
    /// Registers the specified webhook with <see cref="WebhookOptions"/>
    /// </summary>
    public WebhooksFeature RegisterWebhook(Uri endpoint) => RegisterWebhook(new WebhookRegistration(endpoint));
    
    /// <summary>
    /// Registers the specified webhook with <see cref="WebhookOptions"/>
    /// </summary>
    public WebhooksFeature RegisterWebhook(WebhookRegistration registration) => RegisterWebhooks(registration);
    
    /// <summary>
    /// Registers the specified webhooks with <see cref="WebhookOptions"/>
    /// </summary>
    public WebhooksFeature RegisterWebhooks(params WebhookRegistration[] registrations)
    {
        Services.Configure<WebhookOptions>(options => options.WebhookRegistrations.AddRange(registrations));
        return this;
    }

    /// <inheritdoc />
    public override void Apply()
    {
        Services
            .AddHandlersFrom<WebhooksFeature>()
            .AddSingleton<BackgroundWebhookDispatcher>()
            .AddSingleton(WebhookDispatcher)
            .AddSingleton<IWebhookRegistrationService, DefaultWebhookRegistrationService>()
            .AddSingleton<IWebhookRegistrationProvider, OptionsWebhookRegistrationProvider>();

        Services.Configure(ConfigureWebhookOptions);

        var httpClientBuilder = Services.AddHttpClient<IWebhookInvoker, HttpWebhookInvoker>(ConfigureHttpClient);
        ConfigureHttpClientBuilder(httpClientBuilder);
    }
}