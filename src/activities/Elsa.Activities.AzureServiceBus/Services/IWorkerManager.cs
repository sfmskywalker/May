using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Abstractions.Multitenancy;
using Elsa.Models;

namespace Elsa.Activities.AzureServiceBus.Services
{
    public interface IWorkerManager
    {
        Task CreateWorkersAsync(IReadOnlyCollection<Trigger> triggers, ITenant tenant, CancellationToken cancellationToken = default);
        Task CreateWorkersAsync(IReadOnlyCollection<Bookmark> bookmarks, ITenant tenant, CancellationToken cancellationToken = default);
        Task RemoveWorkersAsync(IReadOnlyCollection<Trigger> triggers, CancellationToken cancellationToken = default);
        Task RemoveWorkersAsync(IReadOnlyCollection<Bookmark> bookmarks, CancellationToken cancellationToken = default);
    }
}