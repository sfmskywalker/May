using Elsa.Common.Models;
using Elsa.Extensions;
using Elsa.Workflows.Activities;
using Elsa.Workflows.Contracts;
using Elsa.Workflows.Management.Contracts;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Management.Filters;

namespace Elsa.Workflows.Management.Services;

/// <inheritdoc />
public class WorkflowDefinitionService : IWorkflowDefinitionService
{
    private readonly IWorkflowDefinitionStore _workflowDefinitionStore;
    private readonly IActivityVisitor _activityVisitor;
    private readonly IIdentityGraphService _identityGraphService;
    private readonly Func<IEnumerable<IWorkflowMaterializer>> _materializers;

    /// <summary>
    /// Constructor.
    /// </summary>
    public WorkflowDefinitionService(
        IWorkflowDefinitionStore workflowDefinitionStore,
        IActivityVisitor activityVisitor,
        IIdentityGraphService identityGraphService,
        Func<IEnumerable<IWorkflowMaterializer>> materializers)
    {
        _workflowDefinitionStore = workflowDefinitionStore;
        _activityVisitor = activityVisitor;
        _identityGraphService = identityGraphService;
        _materializers = materializers;
    }

    /// <inheritdoc />
    public async Task<Workflow> MaterializeWorkflowAsync(WorkflowDefinition definition, CancellationToken cancellationToken = default)
    {
        var materializers = _materializers();
        var materializer = materializers.FirstOrDefault(x => x.Name == definition.MaterializerName);

        if (materializer == null)
            throw new Exception("Provider not found");

        var workflow = await materializer.MaterializeAsync(definition, cancellationToken);
        var graph = (await _activityVisitor.VisitAsync(workflow, cancellationToken)).Flatten().ToList();

        // Assign identities.
        _identityGraphService.AssignIdentities(graph);

        return workflow;
    }

    /// <inheritdoc />
    public async Task<WorkflowDefinition?> FindAsync(string definitionId, VersionOptions versionOptions, CancellationToken cancellationToken = default)
    {
        return await FindAsync(definitionId, versionOptions, false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WorkflowDefinition?> FindAsync(string definitionId, VersionOptions versionOptions, bool tenantAgnostic, CancellationToken cancellationToken = default)
    {
        var filter = new WorkflowDefinitionFilter
        {
            DefinitionId = definitionId,
            VersionOptions = versionOptions,
            TenantAgnostic = tenantAgnostic
        };
        return await _workflowDefinitionStore.FindAsync(filter, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WorkflowDefinition?> FindAsync(string definitionVersionId, CancellationToken cancellationToken = default)
    {
        return await FindAsync(definitionVersionId, false, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<WorkflowDefinition?> FindAsync(string definitionVersionId, bool tenantAgnostic, CancellationToken cancellationToken = default)
    {
        var filter = new WorkflowDefinitionFilter
        {
            Id = definitionVersionId,
            TenantAgnostic = tenantAgnostic
        };
        return await _workflowDefinitionStore.FindAsync(filter, cancellationToken);
    }
}