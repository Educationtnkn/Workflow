using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using Elsa.Workflows.Models;
using Microsoft.Extensions.Logging;

namespace AppFW.Utilities.RuleEngine
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
    public class EvaluateRuleActivity : Activity
    {
        [Input] public Input<string> RuleWorkflow { get; set; } = default!;
        [Input] public Input<string> OutputVariable { get; set; } = default!;

        protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
        {
            var ruleEngine = context.GetRequiredService<IRuleEngineExecutionService>();
            var inputBuilder = context.GetRequiredService<IRuleInputBuilder>();
            var logger = context.GetRequiredService<ILogger<EvaluateRuleActivity>>();

            var ruleInputRaw = context.GetVariable<object>("ruleInput");
            logger.LogInformation("[EvaluateRule] ruleInput variable type={Type} value={Value}",
                ruleInputRaw?.GetType()?.FullName ?? "null",
                ruleInputRaw?.ToString() ?? "null");

            var facts = TryGetFactsFromVariable(context)
                ?? TryGetFactsFromWorkflowInput(context)
                ?? new Dictionary<string, object?>();

            var entityId = facts.TryGetValue("entityId", out var eid) ? eid?.ToString() ?? "" : "";
            var entityType = facts.TryGetValue("entityType", out var etype) ? etype?.ToString() ?? "" : "";

            var entityContext = new EntityContext(entityId, entityType, facts);
            var input = inputBuilder.Build(entityContext);

            var workflowName = context.Get(RuleWorkflow) ?? string.Empty;
            var results = await ruleEngine.ExecuteWorkflowAsync(workflowName, input);

            var matched = results
                .Where(r => r.IsSuccess && !string.IsNullOrWhiteSpace(r.SuccessEvent))
                .Select(r => r.SuccessEvent!)
                .ToList();

            logger.LogInformation("[EvaluateRule] Workflow='{Workflow}' — {Count} rule(s) matched: [{Events}]",
                workflowName, matched.Count, string.Join(", ", matched));

            var variableName = context.Get(OutputVariable) ?? "Decisions";
            var csv = string.Join(",", matched);
            context.SetVariable(variableName, csv);

            await context.CompleteActivityAsync();
        }

        private static Dictionary<string, object?>? TryGetFactsFromVariable(ActivityExecutionContext context)
        {
            try
            {
                var ruleInput = context.GetVariable<object>("ruleInput");
                if (ruleInput is null)
                {
                    return null;
                }

                // Handle JsonObject (Elsa stores RunJavaScript setVariable output as JsonObject)
                if (ruleInput is System.Text.Json.Nodes.JsonObject jsonObj)
                {
                    return jsonObj.ToDictionary(
                        p => p.Key,
                        p => (object?)(p.Value switch
                        {
                            System.Text.Json.Nodes.JsonValue val when val.TryGetValue<bool>(out var b) => b,
                            System.Text.Json.Nodes.JsonValue val when val.TryGetValue<long>(out var l) => l,
                            System.Text.Json.Nodes.JsonValue val when val.TryGetValue<double>(out var d) => d,
                            System.Text.Json.Nodes.JsonValue val when val.TryGetValue<string>(out var s) => s,
                            null => null,
                            _ => p.Value?.ToString()
                        })
                    );
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static Dictionary<string, object?>? TryGetFactsFromWorkflowInput(ActivityExecutionContext context)
        {
            var rawInput = context.WorkflowExecutionContext.Input;

            if (rawInput is null || rawInput.Count == 0)
            {
                return null;
            }

            return rawInput.ToDictionary(kv => kv.Key, kv => (object?)kv.Value);
        }
    }
    
}



