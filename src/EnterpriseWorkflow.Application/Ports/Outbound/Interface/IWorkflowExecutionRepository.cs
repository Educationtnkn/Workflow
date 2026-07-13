using EnterpriseWorkflow.Domain.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Outbound.Interface;

public interface IWorkflowExecutionRepository
{

    Task<long> InsertAsync(WorkflowExecution e, CancellationToken ct);

    Task<WorkflowExecution?> GetByExecutionNoUsingEnterpriseInstanceNumberAsync(long EnterpriseInstanceNumber, CancellationToken ct);
    Task UpdateStatusAsync(
long workflowExecutionId, string status, string systemStatus, string businessStatus,
DateTime? endDateTime, string? failureReason, CancellationToken ct);
}
