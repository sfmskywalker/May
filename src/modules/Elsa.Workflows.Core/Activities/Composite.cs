using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Elsa.Expressions.Models;
using Elsa.Workflows.Core.Activities.Flowchart.Models;
using Elsa.Workflows.Core.Attributes;
using Elsa.Workflows.Core.Models;
using Elsa.Workflows.Core.Services;
using Elsa.Workflows.Core.Signals;

namespace Elsa.Workflows.Core.Activities;

/// <summary>
/// Represents a composite activity that has a single <see cref="Root"/> activity. Like a workflow, but without workflow-level properties.
/// </summary>
public abstract class Composite : ActivityBase
{
    /// <inheritdoc />
    protected Composite(string? source = default, int? line = default) : base(source, line)
    {
        OnSignalReceived<CompleteCompositeSignal>(OnCompleteCompositeSignal);
    }
    
    /// <summary>
    /// The activity to schedule when this activity executes.
    /// </summary>
    [Port]
    [Browsable(false)]
    [JsonIgnore] // Composite activities' Root is intended to be constructed from code only, so we don't want to get it serialized.
    public IActivity Root { get; set; } = new Sequence();

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ConfigureActivities(context);
        await context.ScheduleActivityAsync(Root, OnRootCompletedAsync);
    }

    /// <summary>
    /// Override this method to configure activity properties before execution.
    /// </summary>
    protected virtual void ConfigureActivities(ActivityExecutionContext context)
    {
    }

    private async ValueTask OnRootCompletedAsync(ActivityExecutionContext context, ActivityExecutionContext childContext)
    {
        await OnCompletedAsync(context, childContext);
        await context.CompleteActivityAsync();
    }
    
    /// <summary>
    /// Completes this composite activity.
    /// </summary>
    protected async Task CompleteAsync(ActivityExecutionContext context, object? result = default) => await context.SendSignalAsync(new CompleteCompositeSignal(result));
    
    /// <summary>
    /// Completes this composite activity.
    /// </summary>
    protected async Task CompleteAsync(ActivityExecutionContext context, params string[] outcomes) => await CompleteAsync(context, new Outcomes(outcomes));

    protected virtual ValueTask OnCompletedAsync(ActivityExecutionContext context, ActivityExecutionContext childContext)
    {
        OnCompleted(context, childContext);
        return new();
    }

    protected virtual void OnCompleted(ActivityExecutionContext context, ActivityExecutionContext childContext)
    {
    }
    
    private async ValueTask OnCompleteCompositeSignal(CompleteCompositeSignal signal, SignalContext context)
    {
        // Complete the sender first so that it notifies its parents to complete.
        await context.SenderActivityExecutionContext.CompleteActivityAsync();
        
        // Then complete this activity.
        await context.ReceiverActivityExecutionContext.CompleteActivityAsync(signal.Result);
        context.StopPropagation();
    }

    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline From(Func<ActivityExecutionContext, ValueTask> activity, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(activity, source, line);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline From(Func<ValueTask> activity, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(activity, source, line);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline From(Action<ActivityExecutionContext> activity, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(activity, source, line);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline From(Action activity, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(activity, source, line);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline<TResult> From<TResult>(Func<ActivityExecutionContext, ValueTask<TResult>> activity, MemoryBlockReference? output = default, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(activity, output, source, line);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline<TResult> From<TResult>(Func<ValueTask<TResult>> activity, MemoryBlockReference? output = default, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(activity, output, source, line);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline<TResult> From<TResult>(Func<ActivityExecutionContext, TResult> activity, MemoryBlockReference? output, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(activity, output, source, line);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline<TResult> Inline<TResult>(Func<TResult> activity, MemoryBlockReference? output = default, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(activity, output, source, line);

    /// <summary>
    /// Creates a new <see cref="Activities.SetVariable"/> activity.
    /// </summary>
    protected static SetVariable<T> SetVariable<T>(Variable<T> variable, T value, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(variable, value, source, line);
    
    /// <summary>
    /// Creates a new <see cref="Activities.SetVariable"/> activity.
    /// </summary>
    protected static SetVariable<T> SetVariable<T>(Variable<T> variable, Func<ExpressionExecutionContext, T> value, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(variable, value, source, line);
    
    /// <summary>
    /// Creates a new <see cref="Activities.SetVariable"/> activity.
    /// </summary>
    protected static SetVariable<T> SetVariable<T>(Variable<T> variable, Func<T> value, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(variable, value, source, line);
    
    /// <summary>
    /// Creates a new <see cref="Activities.SetVariable"/> activity.
    /// </summary>
    protected static SetVariable<T> SetVariable<T>(Variable<T> variable, Variable<T> value, [CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) => new(variable, value, source, line);
}

/// <summary>
/// Represents a composite activity that has a single <see cref="Root"/> activity and returns a result.
/// </summary>
public abstract class Composite<T> : ActivityBase<T>
{
    /// <inheritdoc />
    protected Composite([CallerFilePath] string? source = default, [CallerLineNumber] int? line = default) : base(source, line)
    {
        OnSignalReceived<CompleteCompositeSignal>(OnCompleteCompositeSignal);
    }

    /// <summary>
    /// The activity to schedule when this activity executes.
    /// </summary>
    [Port]
    [Browsable(false)]
    [JsonIgnore] // Composite activities' Root is intended to be constructed from code only, so we don't want to get it serialized.
    public IActivity Root { get; protected set; } = new Sequence();

    /// <inheritdoc />
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        ConfigureActivities(context);
        await context.ScheduleActivityAsync(Root, OnRootCompletedAsync);
    }

    /// <summary>
    /// Override this method to configure activity properties before execution.
    /// </summary>
    protected virtual void ConfigureActivities(ActivityExecutionContext context)
    {
    }

    /// <summary>
    /// Override this method to handle the completion event for this composite activity.
    /// </summary>
    protected virtual ValueTask OnCompletedAsync(ActivityExecutionContext context, ActivityExecutionContext childContext)
    {
        OnCompleted(context, childContext);
        return new();
    }

    /// <summary>
    /// Override this method to handle the completion event for this composite activity.
    /// </summary>
    protected virtual void OnCompleted(ActivityExecutionContext context, ActivityExecutionContext childContext)
    {
    }

    /// <summary>
    /// Completes this composite activity.
    /// </summary>
    protected async Task CompleteAsync(ActivityExecutionContext context, object? result = default) => await context.SendSignalAsync(new CompleteCompositeSignal(result));
    
    /// <summary>
    /// Completes this composite activity.
    /// </summary>
    protected async Task CompleteAsync(ActivityExecutionContext context, params string[] outcomes) => await CompleteAsync(context, new Outcomes(outcomes));

    private async ValueTask OnRootCompletedAsync(ActivityExecutionContext context, ActivityExecutionContext childContext)
    {
        await OnCompletedAsync(context, childContext);
        await context.CompleteActivityAsync();
    }
    
    private async ValueTask OnCompleteCompositeSignal(CompleteCompositeSignal signal, SignalContext context)
    {
        var activityExecutionContext = context.ReceiverActivityExecutionContext;

        // Remove the existing completed handler.
        activityExecutionContext.WorkflowExecutionContext.PopCompletionCallback(activityExecutionContext, Root);

        // Complete this activity.
        await activityExecutionContext.CompleteActivityAsync(signal.Result);
        context.StopPropagation();
    }

    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline From(Func<ActivityExecutionContext, ValueTask> activity) => new(activity);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline From(Func<ValueTask> activity) => new(activity);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline From(Action<ActivityExecutionContext> activity) => new(activity);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline From(Action activity) => new(activity);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline<TResult> From<TResult>(Func<ActivityExecutionContext, ValueTask<TResult>> activity, MemoryBlockReference? output = default) => new(activity, output);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline<TResult> From<TResult>(Func<ValueTask<TResult>> activity, MemoryBlockReference? output = default) => new(activity, output);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline<TResult> From<TResult>(Func<ActivityExecutionContext, TResult> activity, MemoryBlockReference? output = default) => new(activity, output);
    
    /// <summary>
    /// Creates a new <see cref="Activities.Inline"/> activity.
    /// </summary>
    protected static Inline<TResult> From<TResult>(Func<TResult> activity, MemoryBlockReference? output = default) => new(activity, output);
}