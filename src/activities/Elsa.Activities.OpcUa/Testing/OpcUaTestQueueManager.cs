using Elsa.Activities.OpcUa.Configuration;
using Elsa.Activities.OpcUa.Services;
using Elsa.Models;
using Elsa.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Elsa.Activities.OpcUa.Testing
{
    public class OpcUaTestQueueManager : IOpcUaTestQueueManager
    {
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly IDictionary<string, ICollection<Worker>> _workers;
        private readonly IOpcUaQueueStarter _OpcUaQueueStarter;
        private readonly ILogger _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public OpcUaTestQueueManager(
            IOpcUaQueueStarter OpcUaQueueStarter,
            ILogger<OpcUaTestQueueManager> logger,
            IServiceScopeFactory scopeFactory)
        {
            _OpcUaQueueStarter = OpcUaQueueStarter;
            _logger = logger;
            _workers = new Dictionary<string, ICollection<Worker>>();
            _scopeFactory = scopeFactory;
        }

        public async Task CreateTestWorkersAsync(string workflowId, string workflowInstanceId, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            try
            {

                if (_workers.ContainsKey(workflowInstanceId))
                {
                    if (_workers[workflowInstanceId].Count > 0)
                        return;
                }
                else
                    _workers[workflowInstanceId] = new List<Worker>();

                var workerConfigs = (await GetConfigurationsAsync(workflowId, cancellationToken).ToListAsync(cancellationToken)).Distinct();

                foreach (var config in workerConfigs)
                {
                    try
                    {
                        _workers[workflowInstanceId].Add(await _OpcUaQueueStarter.CreateWorkerAsync(config, cancellationToken));
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e, "Failed to create a test receiver for routing key {RoutingKey}","");
                    }
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DisposeTestWorkersAsync(string workflowInstance)
        {
            foreach (var worker in _workers[workflowInstance])
            {
                await worker.DisposeAsync();
            }

            _workers[workflowInstance].Clear();
        }


        private async IAsyncEnumerable<OpcUaBusConfiguration> GetConfigurationsAsync(string workflowDefinitionId, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();

            var workflowRegistry = scope.ServiceProvider.GetRequiredService<IWorkflowRegistry>();
            var workflowBlueprintReflector = scope.ServiceProvider.GetRequiredService<IWorkflowBlueprintReflector>();
            var workflow = await workflowRegistry.GetWorkflowAsync(workflowDefinitionId, VersionOptions.Latest, cancellationToken);

            if (workflow == null) yield break;

            var workflowBlueprintWrapper = await workflowBlueprintReflector.ReflectAsync(scope.ServiceProvider, workflow, cancellationToken);

            foreach (var activity in workflowBlueprintWrapper.Filter<OpcUaMessageReceived>())
            {
                var connectionString = await activity.EvaluatePropertyValueAsync(x => x.ConnectionString, cancellationToken);
                var Tags = await activity.EvaluatePropertyValueAsync(x => x.Tags, cancellationToken);
                var activityId = activity.ActivityBlueprint.Id;
                var clientId = $"Elsa-{activityId.ToUpper()}";

                var config = new OpcUaBusConfiguration(connectionString!,clientId,  Tags!);

                yield return config;
            }
        }
    }
}