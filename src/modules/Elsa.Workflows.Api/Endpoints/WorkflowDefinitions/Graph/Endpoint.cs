using Elsa.Abstractions;
using Elsa.Expressions.Contracts;
using Elsa.Extensions;
using Elsa.Workflows.Contracts;
using Elsa.Workflows.Management;
using Elsa.Workflows.Serialization.Converters;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Elsa.Workflows.Api.Endpoints.WorkflowDefinitions.Graph;

[PublicAPI]
internal class Graph(IWorkflowDefinitionService workflowDefinitionService, IApiSerializer apiSerializer, IActivityRegistry activityRegistry, IExpressionDescriptorRegistry expressionDescriptorRegistry) : ElsaEndpoint<Request>
{
    public override void Configure()
    {
        Get("/workflow-definitions/subgraph/{id}");
        ConfigurePermissions("read:workflow-definitions");
    }

    public override async Task HandleAsync(Request request, CancellationToken cancellationToken)
    {
        var workflowGraph = await workflowDefinitionService.FindWorkflowGraphAsync(request.Id, cancellationToken);

        if (workflowGraph == null)
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        var parentNode = request.ParentNodeId == null ? workflowGraph.Root : workflowGraph.NodeIdLookup.TryGetValue(request.ParentNodeId, out var node) ? node : workflowGraph.Root;
        var serializerOptions = apiSerializer.GetOptions().Clone();
        serializerOptions.Converters.Add(new ActivityNodeConverter(activityRegistry, expressionDescriptorRegistry, depth: 2));
        await HttpContext.Response.WriteAsJsonAsync(parentNode, serializerOptions, cancellationToken);
    }
}