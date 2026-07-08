using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Domain.Entities.Workflow
{
    public class StartWorkflowRequest
    {
        /// <summary>
        /// The WF_CONFIG_Workflow_Version.Workflow_Version_ID to execute.
        /// </summary>
        public long WorkflowVersionId { get; set; }

        public int OrgId { get; set; }

        public long DomainId { get; set; }

        /// <summary>
        /// The Elsa workflow definition id (or definition version id) mapped to this config version.
        /// Typically stored as a reference somewhere in WF_CONFIG (e.g. a Rule_Reference / external reference column)
        /// or in a small mapping table: WF_CONFIG_Workflow_Version_ID -> Elsa_Workflow_Definition_Id.
        /// </summary>
        public string ElsaWorkflowDefinitionId { get; set; } = default!;

        /// <summary>
        /// What kind of business object this workflow instance is tied to (e.g. "PURCHASE_ORDER", "CLAIM", "ONBOARDING").
        /// Maps to WF_EXEC_Workflow_Instance.Business_Reference_Type_Code.
        /// </summary>
        public string BusinessRefType { get; set; } = default!;

        /// <summary>
        /// The primary key of the business entity in the source system (e.g. Order_ID, Claim_ID).
        /// Maps to WF_EXEC_Workflow_Instance.Business_Reference_ID.
        /// </summary>
        public long BusinessRefId { get; set; }

        /// <summary>
        /// Optional human-readable business reference (e.g. "PO-2026-00451").
        /// Maps to WF_EXEC_Workflow_Instance.Business_Reference_Number.
        /// </summary>
        public string? BusinessRefNumber { get; set; }

        /// <summary>
        /// Source system that triggered this workflow (e.g. "SAP", "SALESFORCE", "PORTAL").
        /// Maps to WF_EXEC_Workflow_Instance.Source_System_Code.
        /// </summary>
        public string? SourceSystemCode { get; set; }

        /// <summary>
        /// Idempotency / tracing id from the calling system.
        /// Maps to WF_EXEC_Workflow_Instance.Source_Request_ID.
        /// </summary>
        public string? SourceRequestId { get; set; }

        /// <summary>
        /// Optional priority hint (maps to Priority_Code).
        /// </summary>
        public string? PriorityCode { get; set; }

        /// <summary>
        /// Optional due date if known up front (SLA can also compute this).
        /// </summary>
        public DateTime? DueDateTime { get; set; }

        /// <summary>
        /// User/service account id initiating this workflow. Maps to Created_By / Updated_By.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Arbitrary business payload to seed Elsa workflow variables/input with
        /// (e.g. customer id, amount, document refs). Keys become Elsa input keys.
        /// </summary>
        public Dictionary<string, object?> Payload { get; set; } = new();
    }
}
