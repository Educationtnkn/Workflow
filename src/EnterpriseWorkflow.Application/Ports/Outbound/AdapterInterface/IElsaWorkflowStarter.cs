using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface
{
    public interface IElsaWorkflowStarter
    {
        Task<ElsaStartResult> StartAsync(
            string workflowDefinitionId,
            string correlationId,
            IDictionary<string, object> input,
            CancellationToken ct);

        /// <summary>
        /// Resume requires the actual Elsa workflow instance id (not just a correlation key)
        /// plus the specific bookmark id that was created when the workflow suspended.
        /// Look these up first via IWorkflowInstanceStore / your Wait_State table.
        /// </summary>
        Task<ElsaResumeResult> ResumeAsync(
            string elsaWorkflowInstanceId,
            string bookmarkId,
            IDictionary<string, object> input,
            CancellationToken ct);

        Task<bool> TryCancelAsync(string elsaWorkflowInstanceId, CancellationToken ct);
    }

    public class ElsaStartResult
    {
        public bool Success { get; set; }
        public string? ElsaWorkflowInstanceId { get; set; }
        public string Status { get; set; } = default!;
        public string? ErrorMessage { get; set; }
    }

    public class ElsaResumeResult
    {
        public bool Success { get; set; }
        public string Status { get; set; } = default!;
        public string? ErrorMessage { get; set; }
    }

}
