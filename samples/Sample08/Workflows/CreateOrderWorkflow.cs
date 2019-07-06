using System;
using System.Net;
using System.Net.Http;
using Elsa.Activities.Email.Activities;
using Elsa.Activities.Http.Activities;
using Elsa.Core.Activities.Primitives;
using Elsa.Core.Expressions;
using Elsa.Services;
using Elsa.Services.Models;
using Sample08.Activities;
using Sample08.Messages;

namespace Sample08.Workflows
{
    public class CreateOrderWorkflow : IWorkflow
    {
        public void Build(IWorkflowBuilder builder)
        {
            builder
                .StartWith<HttpRequestTrigger>(
                    activity =>
                    {
                        activity.Method = HttpMethod.Post.Method;
                        activity.Path = new Uri("/orders", UriKind.RelativeOrAbsolute);
                        activity.ReadContent = true;
                    }
                )
                .Then<SetVariable>(
                    activity =>
                    {
                        activity.VariableName = "order";
                        activity.ValueExpression = new JavaScriptExpression<object>("lastResult().ParsedContent");
                    }
                )
                .Then<SendMassTransitMessage<CreateOrder>>(activity => activity.Message = new JavaScriptExpression<CreateOrder>("order"))
                .Then<Fork>(
                    activity => activity.Forks = new[] { "Write-Response", "Await-Shipment" },
                    fork =>
                    {
                        fork
                            .When("Write-Response")
                            .Then<HttpResponseAction>(
                                activity =>
                                {
                                    activity.Body = new PlainTextExpression("<h1>Order Received</h1><p>Your order has been received. Waiting for shipment.</p>");
                                    activity.ContentType = new PlainTextExpression("text/html");
                                    activity.StatusCode = HttpStatusCode.Accepted;
                                }
                            );

                        fork
                            .When("Await-Shipment")
                            .Then<ReceiveMassTransitMessage<OrderShipped>>()
                            .Then<SendEmail>(
                                activity =>
                                {
                                    activity.From = new PlainTextExpression("shipment@acme.com");
                                    activity.To = new JavaScriptExpression<string>("order.customer.email");
                                    activity.Subject = new JavaScriptExpression<string>("`Your order with ID #${order.id} has been shipped!`");
                                    activity.Body = new JavaScriptExpression<string>(
                                        "`Dear ${order.customer.name}, your order has shipped!`"
                                    );
                                }
                            );
                    }
                );
        }
    }
}