using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Domain.Entities.Workflow
{
    // EnterpriseWorkflow.Domain/Entities/Workflow — new request/response
    public class TriggerEventRequest
    {
        public string EventCode { get; set; } = default!;          // e.g. "CLINICAL_REVIEW"
        public string WorkflowInstanceNumber { get; set; } = default!;
        public string Action { get; set; } = default!;              // "approved" | "rejected"
        public Dictionary<string, object>? Payload { get; set; }
    }

    public class TriggerEventResponse
    {
        public string WorkflowInstanceNumber { get; set; } = default!;
        public string EventCode { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string SubStatus { get; set; } = default!;
    }
}
