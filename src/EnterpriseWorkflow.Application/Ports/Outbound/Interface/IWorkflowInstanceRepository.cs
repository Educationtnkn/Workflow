using EnterpriseWorkflow.Domain.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Outbound.Interface;

public interface IWorkflowInstanceRepository
{

    Task<long> InsertAsync(WorkflowInstance e, CancellationToken ct);
    Task<WorkflowInstance?> GetAsync(long workflowInstanceId, CancellationToken ct);

    Task<WorkflowInstance?> GetByInstanceNumberAsync(string workflowInstanceNumber, CancellationToken ct);

    Task<WorkflowInstance?> GetByInstanceNumberUsingEngineInstanceNumberAsync(string EngineInstanceNumber, CancellationToken ct);

    Task<WorkflowInstance?> GetByPredicateAsync(string predicate, object key, CancellationToken ct);

   Task<CurrentStatusDto> GetCurrentStatusAsync(long workflowInstanceId, CancellationToken ct);

    Task UpdateStatusAsync(
        long workflowInstanceId, string status, string systemStatus, string businessStatus,
        CancellationToken ct, string? failureReason = null);


    Task UpdateEngineWorkflowInstanceAsync(
     string workflowInstanceNumber, string enterpriseInstanceId,
        CancellationToken ct, string? failureReason = null);

}
