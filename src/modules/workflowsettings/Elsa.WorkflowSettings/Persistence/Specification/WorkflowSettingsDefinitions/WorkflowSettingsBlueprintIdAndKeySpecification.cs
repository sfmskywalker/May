using System;
using System.Linq.Expressions;
using Elsa.Persistence.Specifications;
using Elsa.WorkflowSettings.Models;

namespace Elsa.WorkflowSettings.Persistence.Specification.WorkflowSettingsDefinitions
{
    public class WorkflowSettingsBlueprintIdAndKeySpecification : Specification<WorkflowSetting>
    {
        public string WorkflowBlueprintId { get; set; }

        public string Key { get; set; }

        public WorkflowSettingsBlueprintIdAndKeySpecification(string workflowBlueprintId, string key)
        {
            WorkflowBlueprintId = workflowBlueprintId;
            Key = key;
        }

        public override Expression<Func<WorkflowSetting, bool>> ToExpression() => x => x.WorkflowBlueprintId == WorkflowBlueprintId && x.Key == Key;
    }
}