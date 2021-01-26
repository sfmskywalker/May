﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Triggers;

// ReSharper disable once CheckNamespace
namespace Elsa.Activities.Signaling
{
    public class SignalReceivedTrigger : ITrigger
    {
        public string Signal { get; set; } = default!;
        public string? CorrelationId { get; set; }
    }

    public class SignalReceivedWorkflowTriggerProvider : WorkflowTriggerProvider<SignalReceivedTrigger, SignalReceived>
    {
        public override async ValueTask<IEnumerable<ITrigger>> GetTriggersAsync(TriggerProviderContext<SignalReceived> context, CancellationToken cancellationToken) =>
            new[]
            {
                new SignalReceivedTrigger
                {
                    Signal = (await context.Activity.GetPropertyValueAsync(x => x.Signal, cancellationToken))!,
                    CorrelationId = context.ActivityExecutionContext.WorkflowExecutionContext.CorrelationId
                }
            };
    }
}