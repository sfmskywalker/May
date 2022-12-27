using Elsa.Abstractions;
using Elsa.Common.Models;
using Elsa.Workflows.Management.Services;
using Elsa.Workflows.Runtime.Services;

namespace Elsa.Workflows.Api.Endpoints.WorkflowDefinitions.Execute;

public class Execute : ElsaEndpoint<Request, Response>
{
    private readonly IWorkflowDefinitionStore _store;
    private readonly IWorkflowRuntime _workflowRuntime;

    public Execute(IWorkflowDefinitionStore store, IWorkflowRuntime workflowRuntime)
    {
        _store = store;
        _workflowRuntime = workflowRuntime;
    }

    public override void Configure()
    {
        Post("/workflow-definitions/{definitionId}/execute");
        //ConfigurePermissions("exec:workflow-definitions");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var definitionId = request.DefinitionId;
        var exists = await _store.GetExistsAsync(definitionId, VersionOptions.Published, cancellationToken);

        if (!exists)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        var correlationId = request.CorrelationId;
        var startWorkflowOptions = new StartWorkflowRuntimeOptions(correlationId, VersionOptions: VersionOptions.Published);
        var result = await _workflowRuntime.StartWorkflowAsync(definitionId, startWorkflowOptions, cancellationToken);

        if (!HttpContext.Response.HasStarted)
            await SendOkAsync(new Response(result.InstanceId), cancellationToken);
    }
}