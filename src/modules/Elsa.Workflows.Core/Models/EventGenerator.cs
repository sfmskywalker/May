using Elsa.Workflows.Core.Services;

namespace Elsa.Workflows.Core.Models;

/// <summary>
/// Generates events on a workflow instance.
/// </summary>
public abstract class EventGenerator : Trigger, IEventGenerator
{
    /// <inheritdoc />
    protected EventGenerator(string? source = default, int? line = default) : base(source, line)
    {
    }

    /// <inheritdoc />
    protected EventGenerator(string triggerType, int version = 1, string? source = default, int? line = default) : base(triggerType, version, source, line)
    {
    }
}