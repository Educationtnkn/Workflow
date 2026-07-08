namespace AppFW.Utilities.RuleEngine
{
    public sealed class RulesConfigurationException : Exception
    {
        public RulesConfigurationException(string message) : base(message) { }
    }

    public sealed class RulesWorkflowNotFoundException : Exception
    {
        public RulesWorkflowNotFoundException(string workflowName)
            : base($"Workflow '{workflowName}' was not found in loaded rule definitions.") { }
    }

    public sealed class RuleInputValidationException : Exception
    {
        public RuleInputValidationException(string message) : base(message) { }
    }

    public sealed class RuleExecutionException : Exception
    {
        public RuleExecutionException(string message, Exception? inner = null) : base(message, inner) { }
    }
}
