using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Elsa.RuleEngineAdapter
{
    public record RuleEvaluationResult(
      string WorkflowName,
      string RuleName,
      bool IsSuccess,
      string? SuccessEvent,
      string? ErrorMessage,
      object? Output,
      IReadOnlyDictionary<string, object>? Properties = null
      );
    public class RuleInput
    {
        public required Metadata metadata { get; set; }
        public required ExpandoObject facts { get; set; }
        public required Policy policy { get; set; }
    }

    public class Metadata
    {
        public required string domain { get; set; }
        public required string process { get; set; }
        public required string eventTimestamp { get; set; }
    }

    public class Policy
    {
        public required string resolutionStrategy { get; set; }
    }
    public record EntityContext(
        string? EntityId,
        string? EntityType,
        IReadOnlyDictionary<string, object?>? Facts
    )
    {
        public static EntityContext From(
            string? entityId,
            string? entityType,
            IReadOnlyDictionary<string, object?>? facts = null) => new(entityId, entityType, facts ?? new Dictionary<string, object?>()
        );

    }
    public enum ResolutionStrategy
    {
        AllMatches,
        FirstMatchWins,
        HighestPriorityWins
    }
}
