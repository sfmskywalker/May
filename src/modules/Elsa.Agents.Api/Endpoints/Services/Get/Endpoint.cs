﻿using Elsa.Abstractions;
using Elsa.Agents.Persistence.Contracts;
using Elsa.Agents.Persistence.Entities;
using JetBrains.Annotations;

namespace Elsa.Agents.Api.Endpoints.Services.Get;

/// Lists all registered API keys.
[UsedImplicitly]
public class Endpoint(IServiceStore store) : ElsaEndpoint<Request, ServiceDefinition>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/ai/services/{id}");
        ConfigurePermissions("ai/services:read");
    }

    /// <inheritdoc />
    public override async Task<ServiceDefinition> ExecuteAsync(Request req, CancellationToken ct)
    {
        var entity = await store.GetAsync(req.Id, ct);
        
        if(entity == null)
        {
            await SendNotFoundAsync(ct);
            return null!;
        }
        
        return entity;
    }
}