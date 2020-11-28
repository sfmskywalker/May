﻿using Elsa.Activities.Console;
using Elsa.Builders;

namespace Elsa.Samples.CustomAttributesChildWorker.Workflows
{
    public class Customer2Workflow : IWorkflow
    {
        public void Build(IWorkflowBuilder workflow)
        {
            workflow
                .WithCustomAttribute("Customer","Customer2")
                .WriteLine("Specialized workflow for Customer 2");
        }
    }
}