using Elsa.MassTransit.Middleware;
using MassTransit;

namespace Elsa.MassTransit.Extensions;

public static class OtelContextPropagationConfigurationExtensions
{
    public static void ConfigureOtelPropagationMiddleware(this IBusFactoryConfigurator bus, IBusRegistrationContext context)
    {
        bus.ConfigureSend(pipe => pipe.UsePropagationSendMiddleware());
        bus.ConfigurePublish(pipe => pipe.UsePropagationPublishMiddleware());
    }
}