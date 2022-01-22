﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Models;
using Elsa.Services;

namespace Elsa.Activities.AzureServiceBus.Services
{
    public interface IServiceBusTopicsStarter
    {
        Task CreateWorkersAsync(IReadOnlyCollection<WorkflowTrigger> triggers, CancellationToken cancellationToken = default);
        Task CreateWorkersAsync(IReadOnlyCollection<Bookmark> bookmarks, CancellationToken cancellationToken = default);
        Task RemoveWorkersAsync(IReadOnlyCollection<WorkflowTrigger> triggers, CancellationToken cancellationToken = default);
        Task RemoveWorkersAsync(IReadOnlyCollection<Bookmark> bookmarks, CancellationToken cancellationToken = default);
    }
}