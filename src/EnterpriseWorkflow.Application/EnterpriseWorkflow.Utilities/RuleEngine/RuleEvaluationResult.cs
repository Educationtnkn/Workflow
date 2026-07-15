namespace AppFW.Utilities.RuleEngine
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
}

