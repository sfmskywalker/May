using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Models;
using Elsa.ProtoActor.Extensions;
using Elsa.Runtime.Protos;
using Elsa.Workflows.Core.Models;
using Elsa.Workflows.Core.Services;
using Elsa.Workflows.Core.State;
using Elsa.Workflows.Runtime.Services;
using Proto;
using Proto.Cluster;
using Bookmark = Elsa.Workflows.Core.Models.Bookmark;

namespace Elsa.ProtoActor.Grains;

/// <summary>
/// Executes a workflow.
/// </summary>
public class WorkflowGrain : WorkflowGrainBase
{
    private readonly IWorkflowDefinitionService _workflowDefinitionService;
    private readonly IWorkflowRunner _workflowRunner;
    private Workflow _workflow = default!;
    private WorkflowState _workflowState = default!;
    private ICollection<Bookmark> _bookmarks = new List<Bookmark>();

    public WorkflowGrain(IWorkflowDefinitionService workflowDefinitionService, IWorkflowRunner workflowRunner, IContext context) : base(context)
    {
        _workflowDefinitionService = workflowDefinitionService;
        _workflowRunner = workflowRunner;
    }

    public override async Task<StartWorkflowResponse> Start(StartWorkflowRequest request)
    {
        var definitionId = request.DefinitionId;
        var versionOptions = VersionOptions.FromString(request.VersionOptions);
        var correlationId = request.CorrelationId == "" ? default : request.CorrelationId;
        var input = request.Input?.Deserialize();
        var cancellationToken = Context.CancellationToken;

        var workflowDefinition = await _workflowDefinitionService.FindAsync(definitionId, versionOptions, cancellationToken);

        if (workflowDefinition == null)
            return new StartWorkflowResponse
            {
                NotFound = true
            };

        _workflow = await _workflowDefinitionService.MaterializeWorkflowAsync(workflowDefinition, cancellationToken);
        var workflowResult = await _workflowRunner.RunAsync(_workflow, input, cancellationToken);
        var finished = workflowResult.WorkflowState.Status == WorkflowStatus.Finished;

        _workflowState = workflowResult.WorkflowState;
        await StoreBookmarksAsync(workflowResult.Bookmarks, cancellationToken);

        return new StartWorkflowResponse
        {
            Finished = finished
        };
    }

    public override async Task<ResumeWorkflowResponse> Resume(ResumeWorkflowRequest request)
    {
        var input = request.Input?.Deserialize();
        var cancellationToken = Context.CancellationToken;
        var workflowResult = await _workflowRunner.RunAsync(_workflow, _workflowState, input, cancellationToken);
        var finished = workflowResult.WorkflowState.Status == WorkflowStatus.Finished;

        _workflowState = workflowResult.WorkflowState;
        await StoreBookmarksAsync(workflowResult.Bookmarks, cancellationToken);

        return new ResumeWorkflowResponse
        {
            Finished = finished
        };
    }

    private async Task StoreBookmarksAsync(ICollection<Bookmark> bookmarks, CancellationToken cancellationToken)
    {
        _bookmarks = bookmarks;
        var workflowInstanceId = Context.ClusterIdentity()!.Identity;

        foreach (var bookmark in _bookmarks)
        {
            var bookmarkGrainId = bookmark.Hash;
            var bookmarkClient = Context.GetBookmarkGrain(bookmarkGrainId);

            await bookmarkClient.Store(new StoreBookmarkRequest
            {
                BookmarkId = bookmark.Id,
                WorkflowInstanceId = workflowInstanceId
            }, cancellationToken);
        }
    }
}