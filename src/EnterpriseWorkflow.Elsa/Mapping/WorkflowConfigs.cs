// EnterpriseWorkflow.Domain/Configurations/WorkflowConfigs.cs

namespace EnterpriseWorkflow.Domain.Configurations
{
    // ── Inline expression types ──────────────────────────────────────────────

    /// <summary>
    /// How the value of a field is expressed in an action config.
    /// </summary>
    public enum ExpressionType
    {
        Literal,        // plain string/bool/number
        JavaScript,     // JS snippet evaluated at runtime
        Variable        // getVariable('name') shorthand
    }

    /// <summary>
    /// A value that can be either a literal or a JS expression.
    /// </summary>
    public class ExpressionValue
    {
        public ExpressionType ExprType { get; set; } = ExpressionType.Literal;
        public string Value { get; set; } = string.Empty;
    }

    // ── Action configs ───────────────────────────────────────────────────────

    /// <summary>
    /// Elsa.RunJavaScript — run an arbitrary JS snippet.
    /// </summary>
    public class RunJavaScriptConfig
    {
        public string Script { get; set; } = string.Empty;          // always JavaScript expression
    }

    /// <summary>
    /// Elsa.ExecuteWorkflow — call a child workflow by definitionId.
    /// </summary>
    public class ExecuteWorkflowConfig
    {
        public string WorkflowDefinitionId { get; set; } = string.Empty;  // Literal
        /// <summary>
        /// JS expression that returns the input object/dictionary.
        /// e.g. "({ url: '...', method: 'POST', body: {...} })"
        /// </summary>
        public string InputExpression { get; set; } = string.Empty;       // JavaScript
        public string InputTypeName { get; set; } = "Object";             // Object | ObjectDictionary
        public bool WaitForCompletion { get; set; } = true;
    }

    /// <summary>
    /// Elsa.FlowDecision — conditional branch (True/False ports).
    /// </summary>
    public class FlowDecisionConfig
    {
        /// <summary>JS expression returning boolean.</summary>
        public string ConditionExpression { get; set; } = string.Empty;
    }

    /// <summary>
    /// Elsa.SetVariable — set a named workflow variable.
    /// </summary>
    public class SetVariableConfig
    {
        public string VariableId { get; set; } = string.Empty;    // matches variable Id in Variables list
        public string VariableName { get; set; } = string.Empty;
        public string VariableTypeName { get; set; } = "String";  // String | Object | Boolean
        /// <summary>JS or Literal expression for the new value.</summary>
        public ExpressionValue Value { get; set; } = new();
    }

    /// <summary>
    /// Elsa.WriteLine — write a line of text (log).
    /// </summary>
    public class WriteLineConfig
    {
        public ExpressionValue Text { get; set; } = new();
        public string? LogLevel { get; set; } = "Information";
    }

    /// <summary>
    /// Elsa.Event — suspend and wait for a named external event.
    /// </summary>
    public class WaitForEventConfig
    {
        public string EventName { get; set; } = string.Empty;      // Literal
    }

    /// <summary>
    /// Elsa.FlowFork — fan-out to multiple parallel branches (no config needed).
    /// </summary>
    public class FlowForkConfig { }

    /// <summary>
    /// Elsa.FlowJoin — fan-in after parallel branches.
    /// </summary>
    public class FlowJoinConfig
    {
        /// <summary>WaitAll = all branches must reach join; WaitAny = first wins.</summary>
        public string Mode { get; set; } = "WaitAll";   // WaitAll | WaitAny
    }

    /// <summary>
    /// Custom activity — MyApp.EvaluateRuleActivity.
    /// </summary>
    public class EvaluateRuleConfig
    {
        public string RuleWorkflow { get; set; } = string.Empty;   // Literal
        public string OutputVariable { get; set; } = string.Empty; // Literal — variable name to store result
        public bool WaitForCompletion { get; set; } = true;
    }

    /// <summary>
    /// HTTP call via the HttpWithRetry child workflow pattern.
    /// Produces an ExecuteWorkflow activity targeting "HttpWithRetryWorkflow".
    /// </summary>
    public class HttpConfig
    {
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = "POST";
        public object? Body { get; set; }
        public string ContentType { get; set; } = "application/json";
        public int MaxRetries { get; set; } = 3;
        public string RetryDelay { get; set; } = "00:00:05";        // TimeSpan string
        /// <summary>Entity type label passed in the body Input object.</summary>
        public string EntityType { get; set; } = string.Empty;
        /// <summary>JS expression for entityId; defaults to getVariable('entityId').</summary>
        public string EntityIdExpression { get; set; } = "getVariable('entityId')";
    }

    /// <summary>
    /// Approval / human task — maps to ExecuteWorkflow → ManageTaskWorkflow.
    /// </summary>
    public class ApprovalConfig
    {
        public string TaskCategory { get; set; } = string.Empty;   // e.g. PreliminaryReview, Clinical
        /// <summary>JS expression for entityId; defaults to getVariable('entityId').</summary>
        public string EntityIdExpression { get; set; } = "getVariable('entityId')";
        public bool WaitForCompletion { get; set; } = true;
    }
}   