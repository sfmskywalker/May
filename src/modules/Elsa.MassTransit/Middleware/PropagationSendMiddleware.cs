using MassTransit;
using MassTransit.Logging;

namespace Elsa.MassTransit.Middleware;

public class PropagationSendMiddleware() : IFilter<SendContext>
{
    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("propagationSend");
    }

    public async Task Send(SendContext context, IPipe<SendContext> next)
    {
        context.Headers.Set(DiagnosticHeaders.ActivityPropagation, "New");
        await next.Send(context);
    }
}