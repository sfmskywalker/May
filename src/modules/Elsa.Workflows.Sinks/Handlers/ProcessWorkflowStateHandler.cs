using Elsa.Mediator.Models;
using Elsa.Mediator.Services;
using Elsa.Workflows.Sinks.Commands;
using Elsa.Workflows.Sinks.Implementations;
using Elsa.Workflows.Sinks.Services;

namespace Elsa.Workflows.Sinks.Handlers;

/// <summary>
/// A handler for the <see cref="ProcessWorkflowState"/> command.
/// Only used when the <see cref="DefaultWorkflowSinkDispatcher"/> is used, which internally relies on the mediator.
/// </summary>
internal class ProcessWorkflowStateHandler : ICommandHandler<ProcessWorkflowState>
{
    private readonly IWorkflowSinkInvoker _workflowSinkInvoker;

    public ProcessWorkflowStateHandler(IWorkflowSinkInvoker workflowSinkInvoker)
    {
        _workflowSinkInvoker = workflowSinkInvoker;
    }
    
    /// <summary>
    /// Invokes all registered workflow sinks using the workflow sink invoker.
    /// </summary>
    public async Task<Unit> HandleAsync(ProcessWorkflowState command, CancellationToken cancellationToken)
    {
        await _workflowSinkInvoker.InvokeAsync(command.WorkflowState, cancellationToken);
        return Unit.Instance;
    }
}