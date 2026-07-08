using System.Collections.Specialized;
using System.ComponentModel.Design;
using System.Text.RegularExpressions;

namespace AppFW.Utilities.RuleEngine
{
    public enum ResolutionStrategy
    {
        AllMatches,
        FirstMatchWins,
        HighestPriorityWins
    }

    public sealed class RuleEngineOptions
    {
        public string RulesPath { get; set; } = string.Empty;

        public bool FailOnMissingWorkflow { get; set; } = true;

        public Type[] CustomTypes { get; set; } =
        [
            typeof(Regex),
        typeof(DateUtils),
        typeof(CollectionUtils)
        ];

        public ResolutionStrategy ResolutionStrategy { get; set; } = ResolutionStrategy.AllMatches;
    }
}
