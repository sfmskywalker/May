﻿using Elsa.Telnyx.Attributes;

namespace Elsa.Telnyx.Payloads.Call
{
    [Webhook(EventType, ActivityTypeName, "Call Initiated", "Triggered when an incoming call is received.")]
    public sealed record CallInitiatedPayload : CallPayload
    {
        public const string EventType = "call.initiated";
        public const string ActivityTypeName = "CallInitiated";
        public string Direction { get; init; } = default!;
        public string State { get; init; } = default!;
        public string To { get; init; } = default!;
        public string From { get; init; } = default!;
        public DateTimeOffset StartTime { get; set; } = default!;
    }
}