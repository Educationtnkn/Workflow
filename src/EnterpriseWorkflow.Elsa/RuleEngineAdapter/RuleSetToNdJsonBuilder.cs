using EnterpriseWorkflow.Domain.Entities.RuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Elsa.RuleEngineAdapter
{
    public static class RuleSetToNdJsonBuilder
    {
        public static string Build(RuleSetDefinition ruleSet, IEnumerable<string>? ruleNames = null)
        {
            var rules = ruleSet.Rules.Where(r => r.IsActive);
            if (ruleNames is { } names)
                rules = rules.Where(r => names.Contains(r.RuleName));

            var payload = new
            {
                WorkflowName = ruleSet.WorkflowName,
                Rules = rules.OrderBy(r => r.Priority).Select(r => new
                {
                    r.RuleName,
                    Expression = r.EvaluatorReference,   // ⚠️ see syntax note below
                    ErrorMessage = $"Rule '{r.RuleName}' did not match."
                })
            };

            return JsonSerializer.Serialize(payload);
        }
    }
}
