using MassTransit;
using MassTransit.Logging;

namespace Elsa.MassTransit.Middleware;

public class PropagationPublishMiddleware() : IFilter<PublishContext>
{
    public void Probe(ProbeContext context)
    {
        context.CreateFilterScope("propagationPublish");
    }

    public async Task Send(PublishContext context, IPipe<PublishContext> next)
    {
        context.Headers.Set(DiagnosticHeaders.ActivityPropagation, "Link");
        await next.Send(context);
    }
}