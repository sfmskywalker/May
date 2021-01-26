﻿using System;
using System.Linq.Expressions;
using Elsa.Models;

namespace Elsa.Persistence.Specifications.WorkflowTriggers
{
    public class WorkflowInstanceIdSpecification : Specification<WorkflowTrigger>
    {
        public WorkflowInstanceIdSpecification(string workflowInstanceId)
        {
            WorkflowInstanceId = workflowInstanceId;
        }

        public string WorkflowInstanceId { get; }

        public override Expression<Func<WorkflowTrigger, bool>> ToExpression() => trigger => trigger.WorkflowInstanceId == WorkflowInstanceId;
    }
}