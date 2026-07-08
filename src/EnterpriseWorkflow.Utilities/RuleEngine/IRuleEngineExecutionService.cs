using AppFW.Service.Interfaces.DTO;

namespace AppFW.Utilities.RuleEngine
{
    public interface IRuleEngineExecutionService
    {
        IReadOnlyList<string> WorkflowNames { get; }

        Task<IReadOnlyList<RuleEvaluationResult>> ExecuteWorkflowAsync(string workflowName, RuleInput input);

        Task<IReadOnlyDictionary<string, IReadOnlyList<RuleEvaluationResult>>> ExecuteAllWorkflowsAsync(RuleInput input);
    }
}
