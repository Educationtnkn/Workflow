using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Domain.Entities.RuleEngine
{
    public class RuleSetDefinition
    {
        public int RuleSetId { get; set; }
        public string RuleSetName { get; set; } = default!;

        /// <summary>Routing | Join</summary>
        public string RuleSetType { get; set; } = "Routing";

        /// <summary>AllMatches | FirstMatchWins | HighestPriorityWins</summary>
        public string Strategy { get; set; } = "AllMatches";

        /// <summary>Logical name used as MRE WorkflowName — matches ruleWorkflow in EvaluateRuleActivity.</summary>
        public string WorkflowName { get; set; } = default!;

        public List<RuleDefinition> Rules { get; set; } = new();
    }
    public class RuleDefinition
    {
        public int RuleId { get; set; }
        public int RuleSetId { get; set; }
        public string RuleName { get; set; } = default!;
        public int Priority { get; set; } = 1;

        /// <summary>Expression | Script (extensible)</summary>
        public string EvaluationType { get; set; } = "Expression";

        /// <summary>
        /// JsonPath expression: $.case.concerns.clinicalConcern == true
        /// The evaluator translates this to MRE lambda expression syntax.
        /// </summary>
        public string EvaluatorReference { get; set; } = default!;

        /// <summary>
        /// Value stored in FwaDecisions CSV when this rule succeeds.
        /// Defaults to RuleName if not set.
        /// </summary>
        public string? SuccessEvent { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
