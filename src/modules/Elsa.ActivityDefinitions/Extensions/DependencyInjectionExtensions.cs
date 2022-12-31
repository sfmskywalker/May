using Elsa.ActivityDefinitions.Features;
using Elsa.Features.Services;

// ReSharper disable once CheckNamespace
namespace Elsa.Extensions;

public static class DependencyInjectionExtensions
{
    public static IModule UseActivityDefinitions(this IModule module, Action<ActivityDefinitionsFeature>? configure = default)
    {
        module.Configure(configure);
        return module;
    }
}