using Elsa.Common.Features;
using Elsa.Features.Abstractions;
using Elsa.Features.Attributes;
using Elsa.Features.Services;
using Elsa.Scheduling.Contracts;
using Elsa.Scheduling.Handlers;
using Elsa.Scheduling.Services;
using Elsa.Workflows.Management.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Elsa.Scheduling.Features;

/// <summary>
/// Provides scheduling features to the system.
/// </summary>
[DependsOn(typeof(SystemClockFeature))]
public class SchedulingFeature : FeatureBase
{
    /// <inheritdoc />
    public SchedulingFeature(IModule module) : base(module)
    {
    }

    /// <inheritdoc />
    public override void Apply()
    {
        Services
            .AddSingleton<ITriggerScheduler, TriggerScheduler>()
            .AddSingleton<IBookmarkScheduler, BookmarkScheduler>()
            .AddSingleton<IScheduler, LocalScheduler>()
            .AddHandlersFrom<ScheduleWorkflows>();

        Module.Configure<WorkflowManagementFeature>(management => management.AddActivitiesFrom<SchedulingFeature>());
    }
}