using Elsa.Mediator.Contexts;
using Elsa.Mediator.Contracts;
using Elsa.Mediator.Extensions;

namespace Elsa.Mediator.CommandStrategies;

/// <summary>
/// Invokes command handlers using the default strategy. 
/// </summary>
public class DefaultStrategy : ICommandStrategy
{
    /// <inheritdoc />
    public async Task ExecuteAsync(CommandStrategyContext context)
    {
        var command = context.Command;
        var cancellationToken = context.CancellationToken;
        var commandType = command.GetType();
        var handleMethod = commandType.GetCommandHandlerMethod();
        var handler = context.Handler;
        
        await handler.InvokeAsync(handleMethod, command, cancellationToken);
    }
}