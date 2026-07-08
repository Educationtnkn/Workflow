using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Elsa.RuleEngineAdapter
{
    public interface IRuleEngineExecutionService
    {
        IReadOnlyList<string> WorkflowNames { get; }

        Task<IReadOnlyList<RuleEvaluationResult>> ExecuteWorkflowAsync(string workflowName, RuleInput input, IEnumerable<string>? ruleNames = null);

        Task<IReadOnlyDictionary<string, IReadOnlyList<RuleEvaluationResult>>> ExecuteAllWorkflowsAsync(RuleInput input);
    }
}
