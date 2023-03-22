﻿using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Elsa.Expressions.Models;
using Elsa.Extensions;
using Elsa.JavaScript.Contracts;
using Elsa.JavaScript.Notifications;
using Elsa.JavaScript.Options;
using Elsa.Mediator.Contracts;
using Elsa.Workflows.Core.Models;
using Humanizer;
using Jint;
using Microsoft.Extensions.Options;

// ReSharper disable ConvertClosureToMethodGroup

namespace Elsa.JavaScript.Services;

/// <summary>
/// Provides a JavaScript evaluator using Jint.
/// </summary>
public class JintJavaScriptEvaluator : IJavaScriptEvaluator
{
    private readonly IEventPublisher _mediator;
    private readonly JintOptions _jintOptions;

    /// <summary>
    /// Constructor.
    /// </summary>
    public JintJavaScriptEvaluator(IEventPublisher mediator, IOptions<JintOptions> scriptOptions)
    {
        _mediator = mediator;
        _jintOptions = scriptOptions.Value;
    }

    /// <inheritdoc />
    public async Task<object?> EvaluateAsync(string expression,
        Type returnType,
        ExpressionExecutionContext context,
        Action<Engine>? configureEngine = default,
        CancellationToken cancellationToken = default)
    {
        var engine = await GetConfiguredEngine(configureEngine, context, cancellationToken);
        var result = ExecuteExpressionAndGetResult(engine, expression);

        return result;
    }

    private async Task<Engine> GetConfiguredEngine(Action<Engine>? configureEngine, ExpressionExecutionContext context, CancellationToken cancellationToken)
    {
        var engine = new Engine(opts =>
        {
            if (_jintOptions.AllowClrAccess)
                opts.AllowClr();
        });

        configureEngine?.Invoke(engine);

        // Add common functions.
        engine.SetValue("getWorkflowInstanceId", (Func<string>)(() => context.GetActivityExecutionContext().WorkflowExecutionContext.Id));
        engine.SetValue("setCorrelationId", (Action<string?>)(value => context.GetActivityExecutionContext().WorkflowExecutionContext.CorrelationId = value));
        engine.SetValue("getCorrelationId", (Func<string?>)(() => context.GetActivityExecutionContext().WorkflowExecutionContext.CorrelationId));
        engine.SetValue("setCorrelationId", (Action<string?>)(value => context.GetActivityExecutionContext().WorkflowExecutionContext.CorrelationId = value));
        engine.SetValue("setVariable", (Action<string, object>)((id, value) => context.SetVariable(id, value)));
        engine.SetValue("getVariable", (Func<string, object?>)(id => context.GetVariable(id)));
        engine.SetValue("getInput", (Func<string, object?>)(name => context.GetWorkflowExecutionContext().Input.GetValue(name)));

        // Create variable & input setters and getters for each variable.
        CreateMemoryBlockAccessors(engine, context);

        engine.SetValue("isNullOrWhiteSpace", (Func<string, bool>)(value => string.IsNullOrWhiteSpace(value)));
        engine.SetValue("isNullOrEmpty", (Func<string, bool>)(value => string.IsNullOrEmpty(value)));
        engine.SetValue("parseGuid", (Func<string, Guid>)(value => Guid.Parse(value)));
        engine.SetValue("toJson", (Func<object, string>)(value => Serialize(value)));
        engine.SetValue("getShortGuid", (Func<string, string>)(value => Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "")));
        engine.SetValue("getGuid", (Func<string, Guid>)(value => Guid.NewGuid()));
        engine.SetValue("getGuidString", (Func<string, string>)(value => Guid.NewGuid().ToString()));

        // Add common .NET types.
        engine.RegisterType<DateTime>();
        engine.RegisterType<DateTimeOffset>();
        engine.RegisterType<TimeSpan>();
        engine.RegisterType<Guid>();

        // Allow listeners invoked by the mediator to configure the engine.
        await _mediator.PublishAsync(new EvaluatingJavaScript(engine, context), cancellationToken);

        return engine;
    }

    private static void CreateMemoryBlockAccessors(Engine engine, ExpressionExecutionContext context)
    {
        var variables = GetVariablesInScope(context).Values;

        foreach (var variable in variables)
        {
            var pascalName = variable.Name.Pascalize();
            engine.SetValue($"get{pascalName}", (Func<object?>)(() => variable.Get(context)));
            engine.SetValue($"set{pascalName}", (Action<object?>)(value => variable.Set(context, value)));
        }
    }

    private static IDictionary<string, Variable> GetVariablesInScope(ExpressionExecutionContext context)
    {
        var collectedVariables = new Dictionary<string, Variable>();
        var currentScope = context;

        while (currentScope != null)
        {
            if(!currentScope.TryGetActivityExecutionContext(out var activityExecutionContext))
                break;

            var variables = activityExecutionContext.Variables;

            foreach (var variable in variables)
            {
                if (collectedVariables.ContainsKey(variable.Name))
                    continue;

                collectedVariables.Add(variable.Name, variable);
            }

            currentScope = currentScope.ParentContext;
        }

        return collectedVariables;
    }

    private static object ExecuteExpressionAndGetResult(Engine engine, string expression)
    {
        var result = engine.Evaluate(expression);
        return result.ToObject();
    }

    private static string Serialize(object value)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new JsonStringEnumConverter());

        return JsonSerializer.Serialize(value, options);
    }
}