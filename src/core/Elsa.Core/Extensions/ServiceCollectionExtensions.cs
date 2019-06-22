using Elsa.Core.Expressions;
using Elsa.Core.Serialization;
using Elsa.Core.Serialization.Formatters;
using Elsa.Core.Services;
using Elsa.Core.Services.WorkflowBuilders;
using Elsa.Serialization;
using Elsa.Serialization.Formatters;
using Elsa.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;

namespace Elsa.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWorkflowsCore(this IServiceCollection services)
        {
            services.TryAddSingleton<IClock>(SystemClock.Instance);

            return services
                .AddLogging()
                .AddLocalization()
                .AddSingleton<IIdGenerator, IdGenerator>()
                .AddSingleton<IWorkflowSerializer, WorkflowSerializer>()
                .AddSingleton<ITokenFormatter, JsonTokenFormatter>()
                .AddSingleton<ITokenFormatter, YamlTokenFormatter>()
                .AddSingleton<ITokenFormatter, XmlTokenFormatter>()
                .AddSingleton<IExpressionEvaluator, PlainText>()
                .AddSingleton<IExpressionEvaluator, JavaScript>()
                .AddSingleton<IWorkflowInvoker, WorkflowInvoker>()
                .AddSingleton<IActivityInvoker, ActivityInvoker>()
                .AddSingleton<IActivityResolver, ActivityResolver>()
                .AddSingleton<IWorkflowExpressionEvaluator, WorkflowExpressionEvaluator>()
                .AddSingleton<IWorkflowSerializerProvider, WorkflowSerializerProvider>();
        }

        public static IServiceCollection AddActivity<T>(this IServiceCollection services)
            where T : class, IActivity
        {
            return services
                .AddTransient<T>()
                .AddTransient<IActivity>(sp => sp.GetRequiredService<T>());
        }

        public static IServiceCollection AddWorkflowBuilder(this IServiceCollection services)
        {
            return services.AddTransient<WorkflowBuilder>();
        }
    }
}