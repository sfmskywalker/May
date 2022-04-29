using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Mediator.Services;
using Elsa.Models;
using Elsa.Persistence.Models;
using Elsa.Persistence.Requests;
using Elsa.Runtime.ProtoActor.Extensions;
using Elsa.Runtime.ProtoActor.Implementations;
using Elsa.Runtime.Protos;
using Elsa.Runtime.Services;
using Elsa.Serialization;
using Elsa.Services;
using Elsa.State;
using Proto;
using Bookmark = Elsa.Runtime.Protos.Bookmark;

namespace Elsa.Runtime.ProtoActor.Grains;

/// <summary>
/// Executes a workflow instance.
/// </summary>
public class WorkflowInstanceGrain : WorkflowInstanceGrainBase
{
    private readonly IRequestSender _requestSender;
    private readonly IWorkflowRegistry _workflowRegistry;
    private readonly GrainClientFactory _grainClientFactory;
    private readonly IWorkflowRunner _workflowRunner;
    private readonly IWorkflowInstanceFactory _workflowInstanceFactory;
    private readonly WorkflowSerializerOptionsProvider _workflowSerializerOptionsProvider;

    public WorkflowInstanceGrain(
        IRequestSender requestSender, 
        IWorkflowRegistry workflowRegistry, 
        GrainClientFactory grainClientFactory,
        IWorkflowRunner workflowRunner,
        IWorkflowInstanceFactory workflowInstanceFactory,
        WorkflowSerializerOptionsProvider workflowSerializerOptionsProvider, 
        IContext context) : base(context)
    {
        _requestSender = requestSender;
        _workflowRegistry = workflowRegistry;
        _grainClientFactory = grainClientFactory;
        _workflowRunner = workflowRunner;
        _workflowInstanceFactory = workflowInstanceFactory;
        _workflowSerializerOptionsProvider = workflowSerializerOptionsProvider;
    }

    public override async Task<ExecuteWorkflowInstanceResponse> ExecuteExistingInstance(ExecuteExistingWorkflowInstanceRequest request)
    {
        var workflowInstanceId = request.InstanceId;
        var cancellationToken = Context.CancellationToken;
        var workflowInstance = await _requestSender.RequestAsync(new FindWorkflowInstance(workflowInstanceId), cancellationToken);

        if (workflowInstance == null)
            throw new Exception($"No workflow instance found with ID {workflowInstanceId}");

        var workflowDefinitionId = workflowInstance.DefinitionId;
        var workflow = await _workflowRegistry.FindByDefinitionIdAsync(workflowDefinitionId, VersionOptions.SpecificVersion(workflowInstance.Version), cancellationToken);

        if (workflow == null)
            throw new Exception($"No workflow definition found with ID {workflowDefinitionId}");

        var workflowState = workflowInstance.WorkflowState;
        var bookmarkMessage = request.Bookmark;
        var input = request.Input?.Deserialize();
        var executionResult = await ExecuteAsync(workflow, workflowState, bookmarkMessage, input!, cancellationToken);
        var response = MapResult(executionResult);

        return response;
    }

    public override async Task<ExecuteWorkflowInstanceResponse> ExecuteNewInstance(ExecuteNewWorkflowInstanceRequest request)
    {
        var cancellationToken = Context.CancellationToken;
        var versionOptions = VersionOptions.FromString(request.VersionOptions);
        var workflowDefinitionId = request.DefinitionId;
        var workflowInstance = await _workflowInstanceFactory.CreateAsync(workflowDefinitionId, versionOptions, request.CorrelationId, cancellationToken);
        var workflow = await _workflowRegistry.FindByDefinitionIdAsync(workflowDefinitionId, VersionOptions.SpecificVersion(workflowInstance.Version), cancellationToken);

        if (workflow == null)
            throw new Exception($"No workflow definition found with ID {workflowDefinitionId}");

        var workflowState = workflowInstance.WorkflowState;
        var bookmarkMessage = request.Bookmark;
        var input = request.Input?.Deserialize();
        var executionResult = await ExecuteAsync(workflow, workflowState, bookmarkMessage, input!, cancellationToken);
        var response = MapResult(executionResult);

        return response;
    }

    public override async Task<ExecuteWorkflowInstanceResponse> Execute(ExecuteWorkflowRequest request)
    {
        var cancellationToken = Context.CancellationToken;
        var workflowState = JsonSerializer.Deserialize<WorkflowState>(request.WorkflowState, _workflowSerializerOptionsProvider.CreatePersistenceOptions())!;
        var versionOptions = VersionOptions.FromString(request.VersionOptions);
        var workflowDefinitionId = request.DefinitionId;
        var bookmark = request.Bookmark;
        var input = request.Input?.Deserialize();
        var workflow = await _workflowRegistry.FindByDefinitionIdAsync(workflowDefinitionId, versionOptions, cancellationToken);
        
        if (workflow == null)
            throw new Exception($"No workflow definition found with ID {workflowDefinitionId}");

        var result = await ExecuteAsync(workflow, workflowState, bookmark, input, cancellationToken);
        return MapResult(result);
    }

    private ExecuteWorkflowInstanceResponse MapResult(InvokeWorkflowResult result)
    {
        var bookmarks = result.Bookmarks.Select(x => new Bookmark
        {
            Id = x.Id,
            Hash = x.Hash,
            Payload = x.Data,
            Name = x.Name,
            ActivityId = x.ActivityId,
            ActivityInstanceId = x.ActivityInstanceId,
            CallbackMethodName = x.CallbackMethodName
        });

        var options = _workflowSerializerOptionsProvider.CreatePersistenceOptions();

        var response = new ExecuteWorkflowInstanceResponse
        {
            WorkflowState = new Json
            {
                Text = JsonSerializer.Serialize(result.WorkflowState, options)
            }
        };

        response.Bookmarks.Add(bookmarks);

        return response;
    }

    private async Task<InvokeWorkflowResult> ExecuteAsync(Workflow workflow, WorkflowState workflowState, Bookmark? bookmarkMessage, IDictionary<string, object>? input, CancellationToken cancellationToken)
    {
        if (bookmarkMessage == null)
            return await _workflowRunner.RunAsync(workflow, workflowState, input, cancellationToken);

        var bookmark =
            new Elsa.Models.Bookmark(
                bookmarkMessage.Id,
                bookmarkMessage.Name,
                bookmarkMessage.Hash,
                bookmarkMessage.Payload,
                bookmarkMessage.ActivityId,
                bookmarkMessage.ActivityInstanceId,
                bookmarkMessage.CallbackMethodName);

        return await _workflowRunner.RunAsync(workflow, workflowState, bookmark, input, cancellationToken);
    }
}