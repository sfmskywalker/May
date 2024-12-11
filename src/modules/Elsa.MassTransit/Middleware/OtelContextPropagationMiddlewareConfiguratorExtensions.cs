using JetBrains.Annotations;
using MassTransit;
using MassTransit.Configuration;

namespace Elsa.MassTransit.Middleware;

[UsedImplicitly]
public static class OtelContextPropagationMiddlewareConfiguratorExtensions
{
    public static void UsePropagationSendMiddleware(this ISendPipeConfigurator configurator)
    {
        configurator.AddPipeSpecification(new FilterPipeSpecification<SendContext>(new PropagationSendMiddleware()));
    }
    
    public static void UsePropagationPublishMiddleware(this IPublishPipeConfigurator configurator)
    {
        configurator.AddPipeSpecification(new FilterPipeSpecification<PublishContext>(new PropagationPublishMiddleware()));
    }
}