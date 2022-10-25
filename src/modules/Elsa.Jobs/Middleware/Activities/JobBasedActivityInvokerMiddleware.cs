using System.Threading.Tasks;
using Elsa.Jobs.Services;
using Elsa.Workflows.Core.Middleware.Activities;
using Elsa.Workflows.Core.Models;
using Elsa.Workflows.Core.Pipelines.ActivityExecution;
using Elsa.Workflows.Core.Services;
using Elsa.Workflows.Management.Services;

namespace Elsa.Jobs.Middleware.Activities;

public static class JobBasedActivityInvokerMiddlewareExtensions
{
    public static IActivityExecutionBuilder UseJobBasedActivityInvoker(this IActivityExecutionBuilder builder) => builder.UseMiddleware<JobBasedActivityInvokerMiddleware>();
}

/// <summary>
/// Executes the current activity from a background job if the activity is of kind <see cref="ActivityKind.Job"/> or <see cref="ActivityKind.Task"/> 
/// </summary>
public class JobBasedActivityInvokerMiddleware : DefaultActivityInvokerMiddleware
{
    private readonly IActivityRegistry _activityRegistry;
    private readonly IActivityDescriber _activityDescriber;
    private readonly IJobFactory _jobFactory;
    private readonly IJobQueue _jobQueue;

    public JobBasedActivityInvokerMiddleware(
        ActivityMiddlewareDelegate next, 
        IActivityRegistry activityRegistry, 
        IActivityDescriber activityDescriber,
        IJobFactory jobFactory,
        IJobQueue jobQueue) : base(next)
    {
        _activityRegistry = activityRegistry;
        _activityDescriber = activityDescriber;
        _jobFactory = jobFactory;
        _jobQueue = jobQueue;
    }

    protected override async ValueTask ExecuteActivityAsync(ActivityExecutionContext context)
    {
        var activity = context.Activity;
        var activityDescriptor = _activityRegistry.Find(activity.Type) ?? await _activityDescriber.DescribeActivityAsync(activity.GetType(), context.CancellationToken);
        var kind = activityDescriptor.Kind;
        var shouldRunInBackground = kind is ActivityKind.Job or ActivityKind.Task; // TODO: If Task, check the activity's configuration itself to see if the user configured async or sync mode.

        if (!shouldRunInBackground)
        {
            await base.ExecuteActivityAsync(context);
            return;
        }
        
        // Schedule a job that then executes this activity within the context of the entire workflow.
        //context.CreateBookmark()
    }
}