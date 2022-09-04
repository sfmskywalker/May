using System.Text.Json;
using System.Text.Json.Serialization;
using EFCore.BulkExtensions;
using Elsa.Common.Services;
using Elsa.Workflows.Core.Serialization;
using Elsa.Workflows.Core.State;
using Elsa.Workflows.Runtime.Services;
using Microsoft.EntityFrameworkCore;

namespace Elsa.Workflows.Persistence.EntityFrameworkCore.Implementations;

public class EFCoreWorkflowStateStore : IWorkflowStateStore
{
    private readonly IDbContextFactory<WorkflowsDbContext> _dbContextFactory;
    private readonly SerializerOptionsProvider _serializerOptionsProvider;
    private readonly ISystemClock _systemClock;

    public EFCoreWorkflowStateStore(IDbContextFactory<WorkflowsDbContext> dbContextFactory, SerializerOptionsProvider serializerOptionsProvider, ISystemClock systemClock)
    {
        _dbContextFactory = dbContextFactory;
        _serializerOptionsProvider = serializerOptionsProvider;
        _systemClock = systemClock;
    }

    public async ValueTask SaveAsync(string id, WorkflowState state, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var options = _serializerOptionsProvider.CreatePersistenceOptions(ReferenceHandler.Preserve);
        var json = JsonSerializer.Serialize(state, options);
        var now = _systemClock.UtcNow;
        var entry = dbContext.Entry(state);
        var record = await dbContext.WorkflowStates.FirstOrDefaultAsync(x => x.Id == state.Id, cancellationToken);

        if (record == null) 
            entry.Property<DateTimeOffset>("CreatedAt").CurrentValue = now;

        entry.Property<string>("Data").CurrentValue = json;
        entry.Property<DateTimeOffset>("UpdatedAt").CurrentValue = now;
        entry.Property<string>("DefinitionId").CurrentValue = state.WorkflowIdentity.DefinitionId;
        entry.Property<int>("Version").CurrentValue = state.WorkflowIdentity.Version;

        var entities = new[] { state };
        await dbContext.BulkInsertOrUpdateAsync(entities, new BulkConfig { EnableShadowProperties = true }, cancellationToken: cancellationToken);
    }

    public async ValueTask<WorkflowState?> LoadAsync(string id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var options = _serializerOptionsProvider.CreatePersistenceOptions(ReferenceHandler.Preserve);
        var entity = await dbContext.WorkflowStates.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (entity == null)
            return null;
        
        var entry = dbContext.Entry(entity);
        var json = entry.Property<string>("Data").CurrentValue;
        return JsonSerializer.Deserialize<WorkflowState>(json, options);
    }
}