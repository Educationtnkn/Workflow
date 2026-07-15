using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Elsa.RuleEngineAdapter
{
    public interface IRuleInputBuilder
    {
        RuleInput Build(EntityContext context);
    }
    public class RuleInputBuilder : IRuleInputBuilder
    {
        public RuleInput Build(EntityContext context)
        {
            var expandoFacts = new ExpandoObject();
            var factsDict = (IDictionary<string, object>)expandoFacts!;

            // Always present
            factsDict["entityId"] = context.EntityId ?? string.Empty;
            factsDict["entityType"] = context.EntityType ?? string.Empty;

            // Caller-provided facts — convert JsonElement to proper .NET primitives
            // so rule engine string/number comparisons work correctly
            foreach (var (key, value) in context.Facts ?? new Dictionary<string, object?>())
            {
                factsDict[key] = ConvertValue(value) ?? string.Empty;
            }


            return new RuleInput
            {
                metadata = new Metadata
                {
                    domain = "workflow",
                    process = "dynamic-steps",
                    eventTimestamp = DateTimeOffset.UtcNow.ToString("O")
                },
                policy = new Policy { resolutionStrategy = "AllMatches" },
                facts = expandoFacts
            };
        }

        // Workflow input and stored facts arrive as JsonElement after JSON round-trip.
        // Rule engine GlobalParam expressions need proper .NET primitives to compare correctly.
        private static object? ConvertValue(object? value) => value switch
        {
            JsonElement je => ConvertJsonElement(je),
            _ => value
        };

        private static object? ConvertJsonElement(JsonElement je) => je.ValueKind switch
        {
            JsonValueKind.Number when je.TryGetInt64(out var l) => l,
            JsonValueKind.Number => je.GetDouble(),
            JsonValueKind.String => je.GetString(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }
}