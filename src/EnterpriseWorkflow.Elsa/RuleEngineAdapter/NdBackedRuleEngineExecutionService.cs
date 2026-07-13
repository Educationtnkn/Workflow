using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using Microsoft.Extensions.Logging;
using NdRulesEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Elsa.RuleEngineAdapter
{
    // Generic Elsa activity that evaluates a named MRE workflow against the current workflow
    // dispatch input and stores all matched SuccessEvents as a comma-separated string variable.
    //
    // Multiple rules can match simultaneously (AllMatches strategy), making this suitable for
    // fan-out decisions like spawning parallel SME tasks where several conditions can be true at once.
    //
    // Usage in workflow JSON:
    //   type: "MyApp.EvaluateRuleActivity"
    //   ruleWorkflow: "fwa-case-routing"
    //   outputVariable: "FwaDecisions"
    //
    // Downstream FlowDecision conditions:
    //   (getVariable('FwaDecisions') || '').includes('ClinicalConcern')

    [Activity("MyApp", "Rules", "Evaluates a named rule workflow against dispatch input and stores matched SuccessEvents in a workflow variable.")]
    public sealed class NdBackedRuleEngineExecutionService : IRuleEngineExecutionService
    {
        private readonly IRuleRepository _ruleRepo;
        private readonly INdRuleEngine _ndEngine;

        public IReadOnlyList<string> WorkflowNames => Array.Empty<string>(); // dynamic, same as today

        public NdBackedRuleEngineExecutionService(IRuleRepository ruleRepo, INdRuleEngine ndEngine)
        {
            _ruleRepo = ruleRepo;
            _ndEngine = ndEngine;
        }

        public async Task<IReadOnlyList<RuleEvaluationResult>> ExecuteWorkflowAsync(
            string workflowName, RuleInput input, IEnumerable<string>? ruleNames = null)
        {
            var ruleSet = await _ruleRepo.GetByWorkflowNameAsync(workflowName)
                ?? throw new Exception($"RuleSet not found for '{workflowName}'");

            // Build/refresh NdRulesEngine's JSON workflow from your DB rule set
            var json = RuleSetToNdJsonBuilder.Build(ruleSet, ruleNames);
            _ndEngine.LoadWorkflowFromJson(json);

            // NdRulesEngine wants a flat Dictionary<string, object?> — flatten your ExpandoObject facts
            var flatInput = ((IDictionary<string, object?>)input.facts)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            var ndResults = await _ndEngine.ExecuteAsync(workflowName, flatInput);

            var mapped = ndResults.Select(r => new RuleEvaluationResult(
                workflowName, r.RuleName, r.IsSuccess,
                SuccessEvent: r.RuleName,          // or a custom property if NdRuleResult carries one
                ErrorMessage: r.IsSuccess ? null : r.ErrorMessage,
                Output: null,
                Properties: null
            )).ToList();

            var strategy = Enum.TryParse<ResolutionStrategy>(ruleSet.Strategy, true, out var s)
                ? s : ResolutionStrategy.AllMatches;

            return ApplyResolutionStrategy(mapped, strategy); // reuse the same logic already in DbBackedRuleEngineExecutionService
        }

        public async Task<IReadOnlyDictionary<string, IReadOnlyList<RuleEvaluationResult>>> ExecuteAllWorkflowsAsync(RuleInput input)
        {
            var all = await _ruleRepo.GetAllAsync();
            var output = new Dictionary<string, IReadOnlyList<RuleEvaluationResult>>();
            foreach (var rs in all)
                output[rs.WorkflowName] = await ExecuteWorkflowAsync(rs.WorkflowName, input);
            return output;
        }

        private static IReadOnlyList<RuleEvaluationResult> ApplyResolutionStrategy(
           List<RuleEvaluationResult> results, ResolutionStrategy strategy)
        {
            if (strategy == ResolutionStrategy.AllMatches) return results;

            var successful = results.Where(r => r.IsSuccess).ToList();
            if (successful.Count == 0) return Array.Empty<RuleEvaluationResult>();

            if (strategy == ResolutionStrategy.HighestPriorityWins)
                return new[] { successful.OrderByDescending(GetPriority).First() };

            return strategy == ResolutionStrategy.FirstMatchWins
                ? new[] { successful.First() }
                : results;
        }

        private static int GetPriority(RuleEvaluationResult r) =>
          r.Properties?.TryGetValue("Priority", out var raw) == true
          && int.TryParse(raw?.ToString(), out var p) ? p : 0;
    }

}



