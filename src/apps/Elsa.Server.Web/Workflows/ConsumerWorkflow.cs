using System.Dynamic;
using System.Text.Json;
using Elsa.Kafka.Activities;
using Elsa.Workflows;
using Elsa.Workflows.Activities;

namespace Elsa.Server.Web.Workflows;

public class ConsumerWorkflow : WorkflowBase
{
    protected override void Build(IWorkflowBuilder builder)
    {
        var message = builder.WithVariable<ExpandoObject>();
        builder.Name = "Consumer Workflow";
        builder.Root = new Sequence
        {
            Activities =
            {
                new MessageReceived
                {
                    ConsumerDefinitionId = new("consumer-1"),
                    MessageType = new(typeof(ExpandoObject)),
                    Topics = new(["topic-1"]),
                    Result = new(message),
                    CanStartWorkflow = true
                },
                new WriteLine(c => JsonSerializer.Serialize(message.Get(c)))
            }
        };
    }
}