using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using Microsoft.Extensions.Logging;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Elsa.RuleEngineAdapter
{
    /// <summary>
    /// Replaces the file-based RuleEngineExecutionService.
    /// Fetches RuleSet + Rules from IRuleRepository at runtime,
    /// builds MRE Workflow on-the-fly, and executes.
    /// </summary>
    public sealed class DbBackedRuleEngineExecutionService : IRuleEngineExecutionService
    {
        private readonly IRuleRepository _ruleRepo;
        private readonly ILogger<DbBackedRuleEngineExecutionService> _logger;

        // MRE engine is shared; workflows are added/removed per execution
        private readonly RulesEngine.RulesEngine _engine;

        public IReadOnlyList<string> WorkflowNames => Array.Empty<string>(); // dynamic — not pre-loaded

        private static readonly ReSettings _reSettings = new()
        {
            CustomTypes = new[]
            {
            typeof(Regex)
        }
        };

        public DbBackedRuleEngineExecutionService(
            IRuleRepository ruleRepo,
            ILogger<DbBackedRuleEngineExecutionService> logger)
        {
            _ruleRepo = ruleRepo;
            _logger = logger;

            // Initialize engine with empty workflows — added dynamically per call
            _engine = new RulesEngine.RulesEngine(Array.Empty<Workflow>(), _reSettings);
        }

        public async Task<IReadOnlyList<RuleEvaluationResult>> ExecuteWorkflowAsync(
            string workflowName,
            RuleInput input,
            IEnumerable<string>? ruleNames = null,
            CancellationToken ct = default)
        {
            ValidateInput(input);

            // 1. Fetch RuleSet from repository
            var ruleSet = await _ruleRepo.GetByWorkflowNameAsync(workflowName, ct);

            if (ruleSet is null)
            {
                _logger.LogWarning("[RuleEngine] RuleSet not found for workflowName='{Name}'", workflowName);
                throw new Exception($"[RuleEngine] RuleSet not found for workflowName='{workflowName}'");
            }

            // 2. Build MRE Workflow from domain model
            var workflow = RuleSetToWorkflowBuilder.Build(ruleSet);

            // 3. Optionally filter to specific rule names
            var ruleNamesList = ruleNames?.ToList();
            if (ruleNamesList is { Count: > 0 })
            {
                workflow.Rules = workflow.Rules
                    .Where(r => ruleNamesList.Contains(r.RuleName, StringComparer.Ordinal))
                    .ToArray();

                if (workflow.Rules.Count() == 0)
                {
                    _logger.LogWarning("[RuleEngine] No matching rules found for filter [{Rules}]",
                        string.Join(", ", ruleNamesList));
                    return Array.Empty<RuleEvaluationResult>();
                }
            }

            // 4. Use a unique name to avoid collisions in shared engine
            var execWorkflowName = $"{workflowName}_{Guid.NewGuid():N}";
            workflow.WorkflowName = execWorkflowName;

            _engine.AddOrUpdateWorkflow(workflow);

            var ruleParams = new RuleParameter[]
            {
            new("facts",    input.facts),
            new("metadata", input.metadata),
            new("policy",   input.policy)
            };

            IReadOnlyList<RuleResultTree> results;
            try
            {
                results = await _engine.ExecuteAllRulesAsync(execWorkflowName, ruleParams);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Rule execution failed for workflow '{workflowName}'.", ex);
            }
            finally
            {
                _engine.RemoveWorkflow(execWorkflowName);
            }

            // 5. Map results
            var mapped = results.Select(r => new RuleEvaluationResult(
                workflowName,
                r.Rule.RuleName,
                r.IsSuccess,
                r.Rule.SuccessEvent,
                r.IsSuccess ? null : r.Rule.ErrorMessage,
                r.ActionResult?.Output,
                r.Rule.Properties as IReadOnlyDictionary<string, object>
            )).ToList();

            // 6. Apply resolution strategy from RuleSet
            var strategy = Enum.TryParse<ResolutionStrategy>(ruleSet.Strategy, true, out var parsed)
                ? parsed
                : ResolutionStrategy.AllMatches;

            _logger.LogInformation(
                "[RuleEngine] Workflow='{Workflow}' strategy={Strategy} — {Total} rules, {Matched} matched",
                workflowName, strategy, results.Count, mapped.Count(r => r.IsSuccess));

            return ApplyResolutionStrategy(mapped, strategy);
        }

        public async Task<IReadOnlyDictionary<string, IReadOnlyList<RuleEvaluationResult>>>
            ExecuteAllWorkflowsAsync(RuleInput input, CancellationToken ct = default)
        {
            var all = await _ruleRepo.GetAllAsync(ct);
            var output = new Dictionary<string, IReadOnlyList<RuleEvaluationResult>>();

            foreach (var ruleSet in all)
            {
                output[ruleSet.WorkflowName] =
                    await ExecuteWorkflowAsync(ruleSet.WorkflowName, input, null, ct);
            }

            return output;
        }

        // Keep original non-CT overloads to satisfy IRuleEngineExecutionService
        public Task<IReadOnlyList<RuleEvaluationResult>> ExecuteWorkflowAsync(
            string workflowName, RuleInput input, IEnumerable<string>? ruleNames = null)
            => ExecuteWorkflowAsync(workflowName, input, ruleNames, CancellationToken.None);

        public Task<IReadOnlyDictionary<string, IReadOnlyList<RuleEvaluationResult>>>
            ExecuteAllWorkflowsAsync(RuleInput input)
            => ExecuteAllWorkflowsAsync(input, CancellationToken.None);

        // ── Helpers (copied from original, kept identical) ──────────────────

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

        private static void ValidateInput(RuleInput input)
        {
            if (input is null) throw new Exception("Rule input is required.");
            if (input.facts is null) throw new Exception("Rule input.facts is required.");
            if (input.metadata is null) throw new Exception("Rule input.metadata is required.");
            if (input.policy is null) throw new Exception("Rule input.policy is required.");
        }
    }
}