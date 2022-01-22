using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Activities.AzureServiceBus.Bookmarks;
using Elsa.Models;
using Elsa.Persistence;
using Elsa.Persistence.Specifications.Triggers;
using Elsa.Persistence.Specifications.WorkflowInstances;
using Elsa.Services;
using Elsa.Services.Models;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rebus.Extensions;

namespace Elsa.Activities.AzureServiceBus.Services
{
    // TODO: Look for a way to merge ServiceBusQueuesStarter with ServiceBusTopicsStarter - there's a lot of overlap.
    public class ServiceBusTopicsStarter : IServiceBusTopicsStarter
    {
        private readonly ITopicMessageReceiverFactory _receiverFactory;
        private readonly IBookmarkSerializer _bookmarkSerializer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ServiceBusTopicsStarter> _logger;
        private readonly ICollection<TopicWorker> _workers;
        private readonly SemaphoreSlim _semaphore = new(1);

        public ServiceBusTopicsStarter(
            ITopicMessageReceiverFactory receiverFactory,
            IServiceScopeFactory scopeFactory,
            IServiceProvider serviceProvider,
            ILogger<ServiceBusTopicsStarter> logger,
            IBookmarkSerializer bookmarkSerializer)
        {
            _receiverFactory = receiverFactory;
            _scopeFactory = scopeFactory;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _bookmarkSerializer = bookmarkSerializer;
            _workers = new List<TopicWorker>();
        }

        public async Task CreateWorkersAsync(IReadOnlyCollection<WorkflowTrigger> triggers, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            var filteredTriggers = Filter(triggers).ToList();

            try
            {
                foreach (var trigger in filteredTriggers)
                {
                    var bookmark = (TopicMessageReceivedBookmark)trigger.Bookmark;
                    await CreateAndAddWorkerAsync(trigger.WorkflowDefinitionId, bookmark.TopicName, bookmark.SubscriptionName, cancellationToken);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task CreateWorkersAsync(IReadOnlyCollection<Bookmark> bookmarks, CancellationToken cancellationToken = default)
        {
            await _semaphore.WaitAsync(cancellationToken);

            var filteredBookmarks = Filter(bookmarks).ToList();

            try
            {
                foreach (var bookmark in filteredBookmarks)
                {
                    var bookmarkModel = _bookmarkSerializer.Deserialize<TopicMessageReceivedBookmark>(bookmark.Model);
                    await CreateAndAddWorkerAsync(bookmark.WorkflowInstanceId, bookmarkModel.TopicName, bookmarkModel.SubscriptionName, cancellationToken);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task RemoveWorkersAsync(IReadOnlyCollection<WorkflowTrigger> triggers, CancellationToken cancellationToken = default)
        {
            var workflowDefinitionIds = Filter(triggers).Select(x => x.WorkflowDefinitionId).Distinct().ToList();

            var workers =
                from worker in _workers
                from workflowId in workflowDefinitionIds
                where worker.Tag == workflowId
                select worker;

            foreach (var worker in workers.ToList())
            {
                await worker.DisposeAsync();
                _workers.Remove(worker);
            }
        }

        public async Task RemoveWorkersAsync(IReadOnlyCollection<Bookmark> bookmarks, CancellationToken cancellationToken = default)
        {
            var workflowInstanceIds = Filter(bookmarks).Select(x => x.WorkflowInstanceId).Distinct().ToList();
            await RemoveWorkersAsync(workflowInstanceIds);
        }

        private async Task CreateAndAddWorkerAsync(string tag, string topicName, string subscriptionName, CancellationToken cancellationToken)
        {
            try
            {
                var receiver = await _receiverFactory.GetTopicReceiverAsync(topicName, subscriptionName, cancellationToken);
                var worker = ActivatorUtilities.CreateInstance<TopicWorker>(_serviceProvider, tag, receiver, (Func<IReceiverClient, Task>)DisposeReceiverAsync);
                _workers.Add(worker);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to create a receiver for topic {TopicName} and subscription {SubscriptionName}", topicName, subscriptionName);
            }
        }

        private async Task RemoveWorkersAsync(IEnumerable<string> tags)
        {
            var workers =
                from worker in _workers
                from tag in tags
                where worker.Tag == tag
                select worker;

            foreach (var worker in workers.ToList())
                await RemoveWorkerAsync(worker);
        }

        private async Task RemoveWorkerAsync(TopicWorker worker)
        {
            await worker.DisposeAsync();
            _workers.Remove(worker);
        }

        private async Task DisposeReceiverAsync(IReceiverClient messageReceiver) => await _receiverFactory.DisposeReceiverAsync(messageReceiver);

        private IEnumerable<WorkflowTrigger> Filter(IEnumerable<WorkflowTrigger> triggers) => triggers.Where(x => x.Bookmark is TopicMessageReceivedBookmark);

        private IEnumerable<Bookmark> Filter(IEnumerable<Bookmark> triggers)
        {
            var modeType = typeof(TopicMessageReceivedBookmark).GetSimpleAssemblyQualifiedName();
            return triggers.Where(x => x.ModelType == modeType);
        }
    }
}