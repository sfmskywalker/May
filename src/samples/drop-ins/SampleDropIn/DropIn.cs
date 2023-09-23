using Elsa.DropIns.Core;
using Elsa.Extensions;
using Elsa.Features.Services;
using Elsa.Workflows.Core.Contracts;
using Microsoft.Extensions.DependencyInjection;
using SampleDropIn.Activities;

namespace SampleDropIn;

public class DropIn : IDropIn
{
    public void Install(IModule module)
    {
        module.AddActivitiesFrom<DropIn>();
    }

    public async ValueTask ConfigureAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var activityRegistry = serviceProvider.GetRequiredService<IActivityRegistry>();
        await activityRegistry.RegisterAsync<SampleActivity>(cancellationToken: cancellationToken);
    }
}