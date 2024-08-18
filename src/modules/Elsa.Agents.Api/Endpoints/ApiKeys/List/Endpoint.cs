﻿using Elsa.Abstractions;
using Elsa.Agents.Persistence.Contracts;
using Elsa.Agents.Persistence.Entities;
using Elsa.Models;
using JetBrains.Annotations;

namespace Elsa.Agents.Api.Endpoints.ApiKeys.List;

/// Lists all registered API keys.
[UsedImplicitly]
public class Endpoint(IApiKeyStore store) : ElsaEndpointWithoutRequest<ListResponse<ApiKeyDefinition>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Get("/ai/api-keys");
        ConfigurePermissions("ai/api-keys:read");
    }

    /// <inheritdoc />
    public override async Task<ListResponse<ApiKeyDefinition>> ExecuteAsync(CancellationToken ct)
    {
        var entities = await store.ListAsync(ct);
        return new ListResponse<ApiKeyDefinition>(entities.ToList());
    }
}