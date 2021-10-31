using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Exceptions;
using Elsa.Models;
using Elsa.Options;
using Elsa.Persistence;
using Elsa.Persistence.Specifications;
using Elsa.Persistence.Specifications.WorkflowInstances;
using Elsa.Services.Bookmarks;
using Elsa.Services.Models;
using Elsa.Services.Triggers;
using Medallion.Threading;
using Microsoft.Extensions.Logging;
using Open.Linq.AsyncExtensions;

namespace Elsa.Services.Workflows
{
    public class WorkflowLaunchpad : IWorkflowLaunchpad
    {
        private readonly IWorkflowInstanceStore _workflowInstanceStore;
        private readonly IWorkflowFactory _workflowFactory;
        private readonly IBookmarkFinder _bookmarkFinder;
        private readonly ITriggerFinder _triggerFinder;
        private readonly IDistributedLockProvider _distributedLockProvider;
        private readonly IWorkflowInstanceDispatcher _workflowInstanceDispatcher;
        private readonly IWorkflowInstanceExecutor _workflowInstanceExecutor;
        private readonly IWorkflowRunner _workflowRunner;
        private readonly IWorkflowRegistry _workflowRegistry;
        private readonly IGetsStartActivities _getsStartActivities;
        private readonly ElsaOptions _elsaOptions;
        private readonly ILogger _logger;

        public WorkflowLaunchpad(
            IWorkflowInstanceStore workflowInstanceStore,
            IWorkflowFactory workflowFactory,
            IBookmarkFinder bookmarkFinder,
            ITriggerFinder triggerFinder,
            IDistributedLockProvider distributedLockProvider,
            IWorkflowInstanceDispatcher workflowInstanceDispatcher,
            IWorkflowInstanceExecutor workflowInstanceExecutor,
            IWorkflowRunner workflowRunner,
            IWorkflowRegistry workflowRegistry,
            IGetsStartActivities getsStartActivities,
            ElsaOptions elsaOptions,
            ILogger<WorkflowLaunchpad> logger)
        {
            _workflowInstanceStore = workflowInstanceStore;
            _bookmarkFinder = bookmarkFinder;
            _triggerFinder = triggerFinder;
            _distributedLockProvider = distributedLockProvider;
            _workflowInstanceDispatcher = workflowInstanceDispatcher;
            _elsaOptions = elsaOptions;
            _logger = logger;
            _getsStartActivities = getsStartActivities;
            _workflowRegistry = workflowRegistry;
            _workflowRunner = workflowRunner;
            _workflowInstanceExecutor = workflowInstanceExecutor;
            _workflowFactory = workflowFactory;
        }

        public async Task<IEnumerable<CollectedWorkflow>> FindWorkflowsAsync(WorkflowsQuery query, CancellationToken cancellationToken = default)
        {
            var correlationId = query.CorrelationId;

            if (!string.IsNullOrWhiteSpace(correlationId))
                return await CollectResumableOrStartableCorrelatedWorkflowsAsync(query, cancellationToken);

            if (!string.IsNullOrWhiteSpace(query.WorkflowInstanceId))
                return await CollectSpecificWorkflowInstanceAsync(query, cancellationToken);

            return await CollectResumableAndStartableWorkflowsAsync(query, cancellationToken);
        }

        public async Task<IEnumerable<StartableWorkflow>> FindStartableWorkflowsAsync(WorkflowsQuery query, CancellationToken cancellationToken = default)
        {
            var correlationId = query.CorrelationId ?? Guid.NewGuid().ToString("N");
            var updatedContext = query with { CorrelationId = correlationId };
            await using var lockHandle = await AcquireLockAsync(correlationId, cancellationToken);
            var startableWorkflowDefinitions = await CollectStartableWorkflowsInternalAsync(updatedContext, cancellationToken);
            return await InstantiateStartableWorkflows(startableWorkflowDefinitions, cancellationToken).ToList();
        }

        public async Task<StartableWorkflow?> FindStartableWorkflowAsync(
            string workflowDefinitionId,
            string? activityId,
            string? correlationId = default,
            string? contextId = default,
            string? tenantId = default,
            CancellationToken cancellationToken = default)
        {
            var workflowBlueprint = await _workflowRegistry.GetAsync(workflowDefinitionId, tenantId, VersionOptions.Published, cancellationToken);

            if (workflowBlueprint == null || workflowBlueprint.IsDisabled)
                return null;

            return await FindStartableWorkflowAsync(workflowBlueprint, activityId, correlationId, contextId, tenantId, cancellationToken);
        }

        public async Task<StartableWorkflow?> FindStartableWorkflowAsync(
            string workflowDefinitionId,
            int version,
            string? activityId,
            string? correlationId = default,
            string? contextId = default,
            string? tenantId = default,
            CancellationToken cancellationToken = default)
        {
            var workflowBlueprint = await _workflowRegistry.GetAsync(workflowDefinitionId, tenantId, VersionOptions.SpecificVersion(version), cancellationToken);

            if (workflowBlueprint == null || workflowBlueprint.IsDisabled)
                return null;

            return await FindStartableWorkflowAsync(workflowBlueprint, activityId, correlationId, contextId, tenantId, cancellationToken);
        }

        public async Task<RunWorkflowResult?> FindAndExecuteRestartableWorkflowAsync(
            string workflowDefinitionId,
            string activityId,
            int version,
            string signalRConnectionId,
            string lastWorkflowInstanceId,
            string? tenantId = default,
            CancellationToken cancellationToken = default)
        {
            var workflowBlueprint = await _workflowRegistry.GetAsync(workflowDefinitionId, tenantId, VersionOptions.SpecificVersion(version), cancellationToken);

            if (workflowBlueprint == null)
                return null;
            
            var lastWorkflowInstance = await _workflowInstanceStore.FindAsync(new EntityIdSpecification<WorkflowInstance>(lastWorkflowInstanceId), cancellationToken);

            if (lastWorkflowInstance == null)
                return null;

            var startActivity = workflowBlueprint.Activities.First(x => x.Id == activityId);

            var startableWorkflowDefinition = new StartableWorkflowDefinition(workflowBlueprint, startActivity.Id);
            
            var workflow = await InstantiateStartableWorkflow(startableWorkflowDefinition, cancellationToken);

            var previousActivityData = GetActivityDataFromLastWorkflowInstance(workflow.WorkflowInstance, lastWorkflowInstance, workflowBlueprint, activityId);
            
            //overwrite activity data for activities prior to starting one with data from previous workflow instance
            foreach (var (key, value) in previousActivityData)
            {
                workflow.WorkflowInstance.ActivityData[key] = value;
            }

            workflow.WorkflowInstance.SetMetadata("isTest", true);
            workflow.WorkflowInstance.SetMetadata("signalRConnectionId", signalRConnectionId);

            var previousActivityOutput = previousActivityData.Count == 0 ? null : previousActivityData.First().Value?.GetItem("Output");
            
            return await ExecuteStartableWorkflowAsync(workflow, new WorkflowInput(previousActivityOutput), cancellationToken);
        }

        public async Task<StartableWorkflow?> FindStartableWorkflowAsync(
            IWorkflowBlueprint workflowBlueprint,
            string? activityId,
            string? correlationId = default,
            string? contextId = default,
            string? tenantId = default,
            CancellationToken cancellationToken = default)
        {
            async Task<StartableWorkflow?> CollectWorkflows()
            {
                var startableWorkflowDefinition = await CollectStartableWorkflowInternalAsync(workflowBlueprint, activityId, correlationId, contextId, tenantId, cancellationToken);
                return startableWorkflowDefinition != null ? await InstantiateStartableWorkflow(startableWorkflowDefinition, cancellationToken) : default;
            }
            
            if (string.IsNullOrEmpty(correlationId)) 
                return await CollectWorkflows();
            
            // Acquire a lock on correlation ID to prevent duplicate workflow instances from being created.
            await using var correlationLockHandle = await AcquireLockAsync(correlationId, cancellationToken);
            return await CollectWorkflows();

        }

        public async Task FindAndExecuteStartableWorkflowAsync(
            string workflowDefinitionId,
            string? activityId,
            string? correlationId = default,
            string? contextId = default,
            WorkflowInput? input = default,
            string? tenantId = default,
            CancellationToken cancellationToken = default)
        {
            var workflowBlueprint = await _workflowRegistry.GetAsync(workflowDefinitionId, tenantId, VersionOptions.Published, cancellationToken);

            if (workflowBlueprint == null)
            {
                _logger.LogWarning("Could not find workflow with ID {WorkflowDefinitionId}", workflowDefinitionId);
                return;
            }

            await FindAndExecuteStartableWorkflowAsync(workflowBlueprint, activityId, correlationId, contextId, input, cancellationToken);
        }

        public async Task<RunWorkflowResult> FindAndExecuteStartableWorkflowAsync(
            IWorkflowBlueprint workflowBlueprint,
            string? activityId,
            string? correlationId = default,
            string? contextId = default,
            WorkflowInput? input = default,
            CancellationToken cancellationToken = default)
        {
            var workflowDefinitionId = workflowBlueprint.Id;
            var tenantId = workflowBlueprint.TenantId;

            var startableWorkflow = await FindStartableWorkflowAsync(workflowBlueprint, activityId, correlationId, contextId, tenantId, cancellationToken);

            if (startableWorkflow == null)
                throw new WorkflowException($"Could not start workflow with ID {workflowDefinitionId}");

            return await ExecuteStartableWorkflowAsync(startableWorkflow, input, cancellationToken);
        }

        public async Task ExecutePendingWorkflowsAsync(IEnumerable<CollectedWorkflow> pendingWorkflows, WorkflowInput? input = default, CancellationToken cancellationToken = default)
        {
            foreach (var pendingWorkflow in pendingWorkflows)
                await _workflowInstanceExecutor.ExecuteAsync(pendingWorkflow.WorkflowInstanceId, pendingWorkflow.ActivityId, input, cancellationToken);
        }

        public async Task ExecutePendingWorkflowAsync(CollectedWorkflow collectedWorkflow, WorkflowInput? input = default, CancellationToken cancellationToken = default) =>
            await _workflowInstanceExecutor.ExecuteAsync(collectedWorkflow.WorkflowInstanceId, collectedWorkflow.ActivityId, input, cancellationToken);

        public async Task<RunWorkflowResult> ExecutePendingWorkflowAsync(string workflowInstanceId, string? activityId, WorkflowInput? input = default, CancellationToken cancellationToken = default) =>
            await _workflowInstanceExecutor.ExecuteAsync(workflowInstanceId, activityId, input, cancellationToken);

        public async Task DispatchPendingWorkflowsAsync(IEnumerable<CollectedWorkflow> pendingWorkflows, WorkflowInput? input, CancellationToken cancellationToken = default)
        {
            foreach (var pendingWorkflow in pendingWorkflows)
                await DispatchPendingWorkflowAsync(pendingWorkflow, input, cancellationToken);
        }

        public async Task DispatchPendingWorkflowAsync(CollectedWorkflow collectedWorkflow, WorkflowInput? input, CancellationToken cancellationToken = default) =>
            await _workflowInstanceDispatcher.DispatchAsync(new ExecuteWorkflowInstanceRequest(collectedWorkflow.WorkflowInstanceId, collectedWorkflow.ActivityId, input), cancellationToken);

        public Task DispatchPendingWorkflowAsync(string workflowInstanceId, string? activityId, WorkflowInput? input, CancellationToken cancellationToken = default) =>
            DispatchPendingWorkflowAsync(new CollectedWorkflow(workflowInstanceId, activityId), input, cancellationToken);

        public async Task<RunWorkflowResult> ExecuteStartableWorkflowAsync(StartableWorkflow startableWorkflow, WorkflowInput? input, CancellationToken cancellationToken = default) =>
            await _workflowRunner.RunWorkflowAsync(startableWorkflow.WorkflowBlueprint, startableWorkflow.WorkflowInstance, startableWorkflow.ActivityId, input, cancellationToken);

        public async Task<CollectedWorkflow> DispatchStartableWorkflowAsync(StartableWorkflow startableWorkflow, WorkflowInput? input, CancellationToken cancellationToken = default)
        {
            var pendingWorkflow = new CollectedWorkflow(startableWorkflow.WorkflowInstance.Id, startableWorkflow.ActivityId);
            await DispatchPendingWorkflowAsync(pendingWorkflow, input, cancellationToken);
            return pendingWorkflow;
        }

        public async Task<IEnumerable<CollectedWorkflow>> CollectAndExecuteWorkflowsAsync(WorkflowsQuery query, WorkflowInput? input = default, CancellationToken cancellationToken = default)
        {
            var pendingWorkflows = await FindWorkflowsAsync(query, cancellationToken).ToList();
            await ExecutePendingWorkflowsAsync(pendingWorkflows, input, cancellationToken);
            return pendingWorkflows.Select(x => new CollectedWorkflow(x.WorkflowInstanceId, x.ActivityId));
        }

        public async Task<IEnumerable<CollectedWorkflow>> CollectAndDispatchWorkflowsAsync(WorkflowsQuery query, WorkflowInput? input = default, CancellationToken cancellationToken = default)
        {
            var pendingWorkflows = await FindWorkflowsAsync(query, cancellationToken).ToList();
            await DispatchPendingWorkflowsAsync(pendingWorkflows, input, cancellationToken);
            return pendingWorkflows;
        }

        public async Task<IEnumerable<CollectedWorkflow>> CollectResumableAndStartableWorkflowsAsync(WorkflowsQuery query, CancellationToken cancellationToken)
        {
            var bookmarkResultsQuery = query.Bookmark != null ? await _bookmarkFinder.FindBookmarksAsync(query.ActivityType, query.Bookmark, query.CorrelationId, query.TenantId, cancellationToken) : default;
            var bookmarkResults = bookmarkResultsQuery?.ToList() ?? new List<BookmarkFinderResult>();
            var triggeredPendingWorkflows = bookmarkResults.Select(x => new CollectedWorkflow(x.WorkflowInstanceId, x.ActivityId)).ToList();
            var startableWorkflows = await FindStartableWorkflowsAsync(query, cancellationToken);
            var pendingWorkflows = triggeredPendingWorkflows.Concat(startableWorkflows.Select(x => new CollectedWorkflow(x.WorkflowInstance.Id, x.ActivityId))).Distinct().ToList();

            return pendingWorkflows;
        }

        private async Task<IEnumerable<StartableWorkflowDefinition>> CollectStartableWorkflowsInternalAsync(WorkflowsQuery query, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Triggering workflows using {ActivityType}", query.ActivityType);

            var filter = query.Bookmark;
            var triggers = filter != null ? (await _triggerFinder.FindTriggersAsync(query.ActivityType, filter, query.TenantId, cancellationToken)).ToList() : new List<TriggerFinderResult>();
            var startableWorkflows = new List<StartableWorkflowDefinition>();

            foreach (var trigger in triggers)
            {
                var workflowBlueprint = trigger.WorkflowBlueprint;
                var startableWorkflow = await CollectStartableWorkflowInternalAsync(workflowBlueprint, trigger.ActivityId, query.CorrelationId!, query.ContextId, query.TenantId, cancellationToken);

                if (startableWorkflow != null)
                    startableWorkflows.Add(startableWorkflow);
            }

            return startableWorkflows;
        }

        private async Task<StartableWorkflowDefinition?> CollectStartableWorkflowInternalAsync(
            IWorkflowBlueprint workflowBlueprint,
            string? activityId,
            string? correlationId,
            string? contextId = default,
            string? tenantId = default,
            CancellationToken cancellationToken = default)
        {
            var workflowDefinitionId = workflowBlueprint.Id;

            if (!ValidatePreconditions(workflowDefinitionId, workflowBlueprint))
                return null;

            // Acquire a lock on the workflow definition so that we can ensure singleton-workflows never execute more than one instance. 
            var lockKey = $"execute-workflow-definition:tenant:{tenantId}:workflow-definition:{workflowDefinitionId}";
            await using var workflowDefinitionHandle = await AcquireLockAsync(lockKey, cancellationToken);

            if (workflowBlueprint.IsSingleton)
            {
                if (await GetWorkflowIsAlreadyExecutingAsync(tenantId, workflowDefinitionId))
                {
                    _logger.LogDebug("Workflow {WorkflowDefinitionId} is a singleton workflow and is already running", workflowDefinitionId);
                    return null;
                }
            }

            var startActivities = _getsStartActivities.GetStartActivities(workflowBlueprint).Select(x => x.Id).ToHashSet();
            var startActivityId = activityId == null ? startActivities.FirstOrDefault() : startActivities.Contains(activityId) ? activityId : default;

            if (startActivityId == null)
            {
                _logger.LogWarning("Cannot start workflow {WorkflowDefinitionId} with version {WorkflowDefinitionVersion} because it has no starting activities", workflowBlueprint.Id, workflowBlueprint.Version);
                return null;
            }

            return new StartableWorkflowDefinition(workflowBlueprint, startActivityId, correlationId, contextId);
        }

        private async Task<IEnumerable<StartableWorkflow>> InstantiateStartableWorkflows(IEnumerable<StartableWorkflowDefinition> startableWorkflowDefinitions, CancellationToken cancellationToken)
        {
            var startableWorkflows = new List<StartableWorkflow>();

            foreach (var (workflowBlueprint, activityId, correlationId, contextId) in startableWorkflowDefinitions)
            {
                var workflowInstance = await _workflowFactory.InstantiateAsync(
                    workflowBlueprint,
                    correlationId,
                    contextId,
                    cancellationToken: cancellationToken);
                
                await _workflowInstanceStore.SaveAsync(workflowInstance, cancellationToken);
                startableWorkflows.Add(new StartableWorkflow(workflowBlueprint, workflowInstance, activityId));
            }

            return startableWorkflows;
        }
        
        private async Task<StartableWorkflow> InstantiateStartableWorkflow(StartableWorkflowDefinition startableWorkflowDefinition, CancellationToken cancellationToken)
        {
            var workflowInstance = await _workflowFactory.InstantiateAsync(
                startableWorkflowDefinition.WorkflowBlueprint,
                startableWorkflowDefinition.CorrelationId,
                startableWorkflowDefinition.ContextId,
                cancellationToken: cancellationToken);
                
            await _workflowInstanceStore.SaveAsync(workflowInstance, cancellationToken);
            return new StartableWorkflow(startableWorkflowDefinition.WorkflowBlueprint, workflowInstance, startableWorkflowDefinition.ActivityId);
        }

        private async Task<IEnumerable<CollectedWorkflow>> CollectSpecificWorkflowInstanceAsync(WorkflowsQuery query, CancellationToken cancellationToken)
        {
            var bookmarkResultsQuery = query.Bookmark != null ? await _bookmarkFinder.FindBookmarksAsync(query.ActivityType, query.Bookmark, query.CorrelationId, query.TenantId, cancellationToken) : default;
            bookmarkResultsQuery = bookmarkResultsQuery?.Where(x => x.WorkflowInstanceId == query.WorkflowInstanceId);
            var bookmarkResults = bookmarkResultsQuery?.ToList() ?? new List<BookmarkFinderResult>();
            var pendingWorkflows = bookmarkResults.Select(x => new CollectedWorkflow(x.WorkflowInstanceId, x.ActivityId)).ToList();

            return pendingWorkflows;
        }

        private async Task<IEnumerable<CollectedWorkflow>> CollectResumableOrStartableCorrelatedWorkflowsAsync(WorkflowsQuery query, CancellationToken cancellationToken)
        {
            var correlationId = query.CorrelationId!;
            var existingHandle = AmbientLockContext.CurrentCorrelationLock;
            var handle = existingHandle == null ? await AcquireLockAsync(correlationId, cancellationToken) : default;
            
            try
            {
                var correlatedWorkflowInstances = !string.IsNullOrWhiteSpace(correlationId)
                    ? await _workflowInstanceStore.FindManyAsync(new CorrelationIdSpecification<WorkflowInstance>(correlationId).And(new WorkflowUnfinishedStatusSpecification()), cancellationToken: cancellationToken).ToArray()
                    : Array.Empty<WorkflowInstance>();

                _logger.LogDebug("Found {CorrelatedWorkflowCount} workflows with correlation ID {CorrelationId}", correlatedWorkflowInstances.Length, correlationId);

                var collectedWorkflows = new List<CollectedWorkflow>();
                
                if (correlatedWorkflowInstances.Any())
                {
                    var bookmarkResults = query.Bookmark != null
                        ? await _bookmarkFinder.FindBookmarksAsync(query.ActivityType, query.Bookmark, correlationId, query.TenantId, cancellationToken).ToList()
                        : new List<BookmarkFinderResult>();
                    _logger.LogDebug("Found {BookmarkCount} bookmarks for activity type {ActivityType}", bookmarkResults.Count, query.ActivityType);
                    
                    collectedWorkflows.AddRange(bookmarkResults.Select(x => new CollectedWorkflow(x.WorkflowInstanceId, x.ActivityId)));
                }

                // Look for startable workflows next, but only include those who don't already have a correlated instance.
                var correlatedWorkflowDefinitionIds = correlatedWorkflowInstances.Select(x => x.DefinitionId).ToHashSet();
                var startableWorkflowDefinitions = await CollectStartableWorkflowsInternalAsync(query, cancellationToken);
                var workflowsToInclude = startableWorkflowDefinitions.Where(x => !correlatedWorkflowDefinitionIds.Contains(x.WorkflowBlueprint.Id));
                var startableWorkflows = await InstantiateStartableWorkflows(workflowsToInclude, cancellationToken).ToList();
                collectedWorkflows.AddRange(startableWorkflows.Select(x => new CollectedWorkflow(x.WorkflowInstance.Id, x.ActivityId)));

                return collectedWorkflows;
            }
            finally
            {
                if (handle != null)
                    await handle.DisposeAsync();
            }
        }

        private async Task<IDistributedSynchronizationHandle> AcquireLockAsync(string resource, CancellationToken cancellationToken)
        {
            var handle = await _distributedLockProvider.AcquireLockAsync(resource, _elsaOptions.DistributedLockTimeout, cancellationToken);

            if (handle == null)
                throw new LockAcquisitionException($"Failed to acquire a lock on {resource}");

            return handle;
        }

        private bool ValidatePreconditions(string? workflowDefinitionId, IWorkflowBlueprint? workflowBlueprint)
        {
            if (workflowBlueprint != null)
                return true;

            _logger.LogWarning("No workflow definition {WorkflowDefinitionId} found. Make sure the scheduled workflow definition is published and enabled", workflowDefinitionId);
            return false;
        }

        private async Task<bool> GetWorkflowIsAlreadyExecutingAsync(string? tenantId, string workflowDefinitionId)
        {
            var specification = new TenantSpecification<WorkflowInstance>(tenantId).WithWorkflowDefinition(workflowDefinitionId).And(new WorkflowIsAlreadyExecutingSpecification());
            return await _workflowInstanceStore.FindAsync(specification) != null;
        }
        
        private IDictionary<string, IDictionary<string, object?>> GetActivityDataFromLastWorkflowInstance(WorkflowInstance currentWorkflowInstance, WorkflowInstance lastWorkflowInstance, IWorkflowBlueprint workflowBlueprint, string startingActivityId)
        {
            IDictionary<string, IDictionary<string, object?>> CollectSourceActivityData(string targetActivityId, IDictionary<string, IDictionary<string, object?>> activityDataAccumulator)
            {
                var sourceActivityId = workflowBlueprint.Connections.FirstOrDefault(x => x.Target.Activity.Id == targetActivityId)?.Source.Activity.Id;
            
                if (sourceActivityId == null)
                    return activityDataAccumulator;
            
                activityDataAccumulator.Add(sourceActivityId, lastWorkflowInstance.ActivityData.GetItem(sourceActivityId));
            
                return CollectSourceActivityData(sourceActivityId, activityDataAccumulator);
            }
            
            return CollectSourceActivityData(startingActivityId, new Dictionary<string, IDictionary<string, object?>>());
        }
    }
}