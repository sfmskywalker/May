using Elsa.Features.Abstractions;
using Elsa.Features.Attributes;
using Elsa.Features.Services;

namespace Elsa.Features;

/// <summary>
/// A wrapper for invoking application-specific configuration, ensuring it is invoked lastly.
/// </summary>
[DependsOn(typeof(ElsaFeature))]
public class AppFeature : FeatureBase
{
    public AppFeature(IModule module) : base(module)
    {
    }
    
    public Action<IModule>? Configurator { get; set; }

    public override void Configure()
    {
        Configurator?.Invoke(Module);
    }
}