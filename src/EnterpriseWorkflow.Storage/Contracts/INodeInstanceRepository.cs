using EnterpriseWorkflow.Domain.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Storage.Contracts
{

    public interface INodeInstanceRepository
    {
        Task<NodeInstance> GetOrCreateAsync(
            long workflowInstanceId,
            long configNodeId,
            string nodeTypeValue,
            string nodeInstanceNumber,
            CancellationToken ct);

        Task<NodeInstance?> GetByNumberAsync(string nodeInstanceNumber, CancellationToken ct);

        Task UpdateStatusAsync(
            long nodeInstanceId,
            string status,
            string systemStatus,
            string businessStatus,
            DateTime? completedDateTime,
            string? failureReason,
            CancellationToken ct);
    }

    public interface INodeExecutionRepository
    {
        Task<long> InsertAsync(NodeExecution entity, CancellationToken ct);

        Task<int> NextSequenceAsync(long nodeInstanceId, CancellationToken ct);

        Task UpdateLatestAsync(
            string nodeInstanceNumber,
            string newStatusValue,
            string newSystemStatus,
            string newBusinessStatus,
            DateTime? endDateTime,
            string? failureReason,
            CancellationToken ct);
    }

    //public interface IWorkflowExecutionRepository
    //{
    //    Task<long> InsertAsync(WorkflowExecution entity, CancellationToken ct);

    //    Task UpdateStatusAsync(
    //        long workflowExecutionId,
    //        string status,
    //        string systemStatus,
    //        string businessStatus,
    //        DateTime? endDateTime,
    //        string? failureReason,
    //        CancellationToken ct);
    //}

    //public interface IWorkflowInstanceRepository
    //{
    //    Task<long> InsertAsync(WorkflowInstance entity, CancellationToken ct);

    //    Task<WorkflowInstance?> GetAsync(long workflowInstanceId, CancellationToken ct);

    //    Task<WorkflowInstance?> GetByInstanceNumberAsync(string workflowInstanceNumber, CancellationToken ct);

    //    Task<CurrentStatusDto> GetCurrentStatusAsync(long workflowInstanceId, CancellationToken ct);

    //    Task UpdateStatusAsync(
    //        long workflowInstanceId,
    //        string status,
    //        string systemStatus,
    //        string businessStatus,
    //        CancellationToken ct,
    //        string? failureReason = null);
    //}

    //public interface IStatusHistoryRepository
    //{
    //    Task<long> InsertAsync(StatusHistory entity, CancellationToken ct);

    //    Task<int> NextSequenceAsync(long workflowInstanceId, CancellationToken ct);
    //}

    //public interface IWaitStateRepository
    //{
    //    Task<long> InsertAsync(WorkflowWaitState entity, CancellationToken ct);

    //    Task<WorkflowWaitState?> GetActiveByCorrelationKeyAsync(string correlationKey, CancellationToken ct);

    //    Task MarkResumedAsync(long waitStateId, CancellationToken ct);
    //}
}
