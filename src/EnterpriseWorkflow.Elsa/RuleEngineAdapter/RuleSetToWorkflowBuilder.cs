using EnterpriseWorkflow.Domain.Entities.RuleEngine;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rule = RulesEngine.Models.Rule;

namespace EnterpriseWorkflow.Elsa.RuleEngineAdapter
{
    public static class RuleSetToWorkflowBuilder
    {
        public static Workflow Build(RuleSetDefinition ruleSet)
        {
            var rules = ruleSet.Rules
                .Where(r => r.IsActive)
                .OrderBy(r => r.Priority)
                .Select(BuildRule)
                .ToArray();

            return new Workflow
            {
                WorkflowName = ruleSet.WorkflowName,
                Rules = rules
            };
        }

        private static Rule BuildRule(RuleDefinition rule)
        {
            return new Rule
            {
                RuleName = rule.RuleName,
                RuleExpressionType = RuleExpressionType.LambdaExpression,
                // EvaluatorReference is already MRE-compatible C# lambda expression
                Expression = rule.EvaluatorReference,
                SuccessEvent = rule.SuccessEvent ?? rule.RuleName,
                ErrorMessage = $"Rule '{rule.RuleName}' did not match.",
                Enabled = rule.IsActive,
                Properties = new Dictionary<string, object>
                {
                    ["Priority"] = rule.Priority,
                    ["RuleId"] = rule.RuleId,
                    ["RuleSetId"] = rule.RuleSetId
                }
            };
        }
    }
}
