using Elsa.Caching.Contracts;
using Elsa.Common.Models;
using Elsa.Workflows.Management.Contracts;

namespace Elsa.Workflows.Management.Services;

/// <inheritdoc />
public class WorkflowDefinitionCacheManager(IChangeTokenSignaler changeTokenSignaler) : IWorkflowDefinitionCacheManager
{
    /// <inheritdoc />
    public string CreateWorkflowDefinitionVersionCacheKey(string definitionId, VersionOptions versionOptions) => $"WorkflowDefinition:{definitionId}:{versionOptions}";

    /// <inheritdoc />
    public string CreateWorkflowVersionCacheKey(string definitionId, VersionOptions versionOptions) => $"Workflow:{definitionId}:{versionOptions}";

    /// <inheritdoc />
    public string CreateWorkflowVersionCacheKey(string definitionVersionId) => $"Workflow:{definitionVersionId}";

    /// <inheritdoc />
    public string CreateWorkflowDefinitionVersionCacheKey(string definitionVersionId) => $"WorkflowDefinition:{definitionVersionId}";

    /// <inheritdoc />
    public string CreateWorkflowDefinitionChangeTokenKey(string definitionId) => $"WorkflowChangeToken:{definitionId}";

    /// <inheritdoc />
    public async Task EvictWorkflowDefinitionAsync(string definitionId, CancellationToken cancellationToken = default)
    {
        var changeTokenKey = CreateWorkflowDefinitionChangeTokenKey(definitionId);
        await changeTokenSignaler.TriggerTokenAsync(changeTokenKey, cancellationToken);
    }
}