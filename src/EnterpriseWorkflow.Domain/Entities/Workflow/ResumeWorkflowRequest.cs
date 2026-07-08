using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Domain.Entities.Workflow
{
    public class ResumeWorkflowRequest
    {
        /// <summary>
        /// Either the Workflow_Instance_Number (enterprise correlation key)
        /// or the Wait_Correlation_Key used when the wait state was created.
        /// </summary>
        public string CorrelationKey { get; set; } = default!;

        /// <summary>
        /// The external event/bookmark name Elsa is waiting on (e.g. "ApprovalReceived", "PaymentConfirmed").
        /// </summary>
        public string ExpectedEventNameCode { get; set; } = default!;

        /// <summary>
        /// Data to feed into the resumed activity (e.g. approval decision, payment reference).
        /// </summary>
        public Dictionary<string, object?> ResumeInput { get; set; } = new();

        public int UserId { get; set; }
    }
}
