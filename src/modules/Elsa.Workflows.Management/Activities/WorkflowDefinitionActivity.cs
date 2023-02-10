using Elsa.Common.Models;
using Elsa.Expressions.Helpers;
using Elsa.Extensions;
using Elsa.Workflows.Core.Contracts;
using Elsa.Workflows.Core.Models;
using Elsa.Workflows.Core.Services;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Management.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Elsa.Workflows.Management.Activities;

/// <summary>
/// Loads and executes an <see cref="WorkflowDefinition"/>.
/// </summary>
public class WorkflowDefinitionActivity : Activity, IInitializable
{
    internal IActivity Root { get; set; } = default!;
    
    /// <summary>
    /// The definition ID of the workflow to schedule for execution.
    /// </summary>
    public string WorkflowDefinitionId { get; set; } = default!;

    public IDictionary<string, object> ResolvedInputValues { get; set; } = new Dictionary<string, object>();
    
    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        // Schedule the activity for execution.
        await context.ScheduleActivityAsync(Root, OnChildCompletedAsync);
    }

    private async ValueTask OnChildCompletedAsync(ActivityExecutionContext context, ActivityExecutionContext childContext) => await context.CompleteActivityAsync();
    
    async ValueTask IInitializable.InitializeAsync(InitializationContext context)
    {
        var serviceProvider = context.ServiceProvider;
        var cancellationToken = context.CancellationToken;
        var workflowDefinitionStore = serviceProvider.GetRequiredService<IWorkflowDefinitionStore>();
        var workflowDefinition = await workflowDefinitionStore.FindByDefinitionIdAsync(WorkflowDefinitionId, VersionOptions.Published, cancellationToken);

        if (workflowDefinition == null)
            throw new Exception($"Workflow definition {WorkflowDefinitionId} not found");

        // Construct the root activity stored in the activity definitions.
        var materializer = serviceProvider.GetRequiredService<IWorkflowMaterializer>();
        var root = await materializer.MaterializeAsync(workflowDefinition, cancellationToken);
        
        Root = root;
    }
}