using System.Threading;
using System.Threading.Tasks;
using Elsa.Services.Models;
using NodaTime;

namespace Elsa.Activities.Timers.Services
{
    public interface IWorkflowScheduler
    {
        Task ScheduleWorkflowAsync(string? workflowDefinitionId, string? workflowInstanceId, string activityId, string? tenantId, Instant startAt, Duration? interval = default, CancellationToken cancellationToken = default);
        Task ScheduleWorkflowAsync(string? workflowDefinitionId, string? workflowInstanceId, string activityId, string? tenantId, string cronExpression, CancellationToken cancellationToken = default);
        Task UnscheduleWorkflowAsync(string? workflowDefinitionId, string? workflowInstanceId, string activityId, string? tenantId, CancellationToken cancellationToken = default);
    }
}