using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Domain.Entities.Workflow
{
    public class StartWorkflowResponse
    {
        public long WorkflowInstanceId { get; set; }

        public string WorkflowInstanceNumber { get; set; } = default!;

        public long WorkflowExecutionId { get; set; }

        /// <summary>
        /// Enterprise-side status immediately after dispatch (e.g. "Pending", "Running", "Rejected").
        /// </summary>
        public string Status { get; set; } = default!;

        /// <summary>
        /// Populated only if Elsa failed to start (engine down, definition missing, etc.).
        /// </summary>
        public string? ErrorMessage { get; set; }
    }
}
