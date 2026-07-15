using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using EnterpriseWorkflow.Elsa.RuleEngineAdapter;
using RulesEngine.Models;
using System.Text.RegularExpressions;

namespace AppFW.Utilities.RuleEngine
{
    public class RuleEngineExecutionService : IRuleEngineExecutionService
    {
        // Private fields
        private readonly RulesEngine.RulesEngine _engine;
        private readonly string[] _workflowNames;
        private readonly bool _failOnMissingWorkflow;
        private readonly ResolutionStrategy _resolutionStrategy;

        // Public properties
        public IReadOnlyList<string> WorkflowNames => _workflowNames;

        public RuleEngineExecutionService(string workflowJsonPath)
            : this(new RuleEngineOptions { RulesPath = workflowJsonPath }) { }

        public RuleEngineExecutionService(RuleEngineOptions options)
        {
            if (options is null)
            {
                throw new RulesConfigurationException("RuleEngineOptions are required.");
            }

            if (string.IsNullOrWhiteSpace(options.RulesPath))
            {
                throw new RulesConfigurationException("RuleEngineOptions.RulesPath is required.");
            }

            // Load workflows
            var workflows = RuleLoader.Load(options.RulesPath);
            _workflowNames = workflows.Select(w => w.WorkflowName).ToArray();
            _failOnMissingWorkflow = options.FailOnMissingWorkflow;
            _resolutionStrategy = options.ResolutionStrategy;

            // Inject custom functions
            var reSettings = new ReSettings
            {
                CustomTypes = options.CustomTypes is { Length: > 0 }
                    ? options.CustomTypes
                    : new[] { typeof(Regex), typeof(DateUtils), typeof(CollectionUtils) }
            };

            // Initialize engine
            _engine = new RulesEngine.RulesEngine(workflows, reSettings);
        }

        public static RuleInput DeserializeInput(string inputJson)
        {
            if (string.IsNullOrWhiteSpace(inputJson))
            {
                throw new RuleInputValidationException("Rule input JSON is required.");
            }

            // Deserialize
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new ExpandoObjectConverter() }
            };

            // Deserialize the input JSON into a RuleInput object
            var input = JsonConvert.DeserializeObject<RuleInput>(inputJson, settings);

            if (input == null)
            {
                throw new RuleInputValidationException("Unable to deserialize rule input JSON.");
            }

            return input;
        }

        public async Task<IReadOnlyList<RuleEvaluationResult>> ExecuteWorkflowAsync(string workflowName, RuleInput input)
        {
            ValidateInput(input);

            var workflowExists = _workflowNames.Contains(workflowName, StringComparer.Ordinal);
            if (!workflowExists)
            {
                if (_failOnMissingWorkflow)
                {
                    throw new RulesWorkflowNotFoundException(workflowName);
                }

                return [];
            }

            // Prepare rule parameters
            var ruleParams = new RuleParameter[]
            {
            new("facts",    input.facts),
            new("metadata", input.metadata),
            new("policy",   input.policy)
            };

            IReadOnlyList<RulesEngine.Models.RuleResultTree> results;
            try
            {
                // Execute rules
                results = await _engine.ExecuteAllRulesAsync(workflowName, ruleParams);
            }
            catch (Exception ex)
            {
                throw new RuleExecutionException($"Rule execution failed for workflow '{workflowName}'.", ex);
            }

            // Map results to our custom result type
            var mapped = results.Select(result => new RuleEvaluationResult(
                        workflowName,
                        result.Rule.RuleName,
                        result.IsSuccess,
                        result.Rule.SuccessEvent,
                        result.IsSuccess ? null : result.Rule.ErrorMessage,
                        result.ActionResult?.Output,
                        result.Rule.Properties as IReadOnlyDictionary<string, object>
            )).ToList();

            var strategy = ResolveStrategy(input);
            return ApplyResolutionStrategy(mapped, strategy);
        }

        public async Task<IReadOnlyDictionary<string, IReadOnlyList<RuleEvaluationResult>>> ExecuteAllWorkflowsAsync(RuleInput input)
        {
            ValidateInput(input);

            // Prepare output dictionary
            var output = new Dictionary<string, IReadOnlyList<RuleEvaluationResult>>();

            // Execute each workflow sequentially
            foreach (var workflowName in _workflowNames)
            {
                output[workflowName] = await ExecuteWorkflowAsync(workflowName, input);
            }

            // Return results
            return output;
        }

        private ResolutionStrategy ResolveStrategy(RuleInput input)
        {
            // Check if strategy is specified in input.policy.resolutionStrategy
            var raw = input.policy.resolutionStrategy?.Trim();

            // If specified and valid, use it; otherwise, fall back to configured strategy
            if (!string.IsNullOrWhiteSpace(raw) && Enum.TryParse<ResolutionStrategy>(raw, ignoreCase: true, out var parsed))
            {
                return parsed;
            }

            return _resolutionStrategy;
        }

        public IReadOnlyList<RuleEvaluationResult> ApplyResolutionStrategy(List<RuleEvaluationResult> results, ResolutionStrategy strategy)
        {
            // If all matches, return all results
            if (strategy == ResolutionStrategy.AllMatches)
            {
                return results;
            }

            var successfulResults = results.Where(r => r.IsSuccess).ToList();
            if (successfulResults.Count == 0)
            {
                return [];
            }

            if (strategy == ResolutionStrategy.HighestPriorityWins)
            {
                var winner = successfulResults
                    .OrderByDescending(GetPriority)
                    .First();

                Console.WriteLine($"[RuleEngine] Winner Rule={winner.RuleName}, Priority={GetPriority(winner)}");

                return [winner];
            }

            return strategy switch
            {
                ResolutionStrategy.FirstMatchWins => [successfulResults.First()],
                _ => results
            };
        }

        private static int GetPriority(RuleEvaluationResult result)
        {
            if (result.Properties is not null && result.Properties.TryGetValue("Priority", out var raw) && raw is not null && int.TryParse(raw.ToString(), out var priority))
            {
                return priority;
            }

            return 0;
        }


        private static void ValidateInput(RuleInput input)
        {
            if (input is null)
            {
                throw new RuleInputValidationException("Rule input is required.");
            }

            if (input.facts is null)
            {
                throw new RuleInputValidationException("Rule input.facts is required.");
            }

            if (input.metadata is null)
            {
                throw new RuleInputValidationException("Rule input.metadata is required.");
            }

            if (input.policy is null)
            {
                throw new RuleInputValidationException("Rule input.policy is required.");
            }
        }
    }
}


