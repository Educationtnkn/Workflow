using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Domain.Exceptions
{
    public class WorkflowValidationException : Exception
    {
        public WorkflowValidationException(string message) : base(message) { }
        public WorkflowValidationException(string message, Exception inner) : base(message, inner) { }
    }

    public class WorkflowNotFoundException : Exception
    {
        public string? DefinitionId { get; }
        public string? ExecutionId { get; }

        public WorkflowNotFoundException(string id) : base($"Workflow not found: {id}")
        {
            if (id.StartsWith("wf_"))
                ExecutionId = id;
            else
                DefinitionId = id;
        }
    }

    public class WorkflowEngineException : Exception
    {
        public string EngineType { get; }

        public WorkflowEngineException(string engineType, string message) : base(message)
        {
            EngineType = engineType;
        }
    }
}
