using System.Reflection;

using Elsa.Services;
using Elsa.Triggers;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {    
        public static IServiceCollection AddWorkflowProvider<T>(this IServiceCollection services) where T : class, IWorkflowProvider => services.AddTransient<IWorkflowProvider, T>();
        public static IServiceCollection AddTriggerProvider<T>(this IServiceCollection services) where T : class, ITriggerProvider => services.AddTransient<ITriggerProvider, T>();

        /// <summary>
        /// Add all TriggerProviders (<see cref="ITriggerProvider"/>) that are in the assembly
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IServiceCollection AddTriggerProvider(this IServiceCollection services, Assembly assembly)
        {
            var ITriggerProviderType = typeof(ITriggerProvider);
            var types = assembly.GetAllWithInterface(ITriggerProviderType);

            foreach (var type in types)
            {
                services.AddTransient(ITriggerProviderType, type);
            }

            return services;
        }

        public static IServiceCollection AddWorkflowContextProvider<T>(this IServiceCollection services) where T : class, IWorkflowContextProvider => services.AddTransient<IWorkflowContextProvider, T>();

        /// <summary>
        /// Add all WorkflowContextProviders (<see cref="IWorkflowContextProvider"/>) that are in the assembly
        /// </summary>
        /// <param name="services"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IServiceCollection AddWorkflowContextProvider(this IServiceCollection services, Assembly assembly)
        {
            var iWorkflowContextProviderType = typeof(IWorkflowContextProvider);
            var types = assembly.GetAllWithInterface(iWorkflowContextProviderType);

            foreach (var type in types)
            {
                services.AddTransient(iWorkflowContextProviderType, type);
            }

            return services;
        }
    }
}