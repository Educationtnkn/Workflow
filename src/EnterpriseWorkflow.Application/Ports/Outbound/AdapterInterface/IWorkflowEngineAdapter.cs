
using EnterpriseWorkflow.Application.Model;
using EnterpriseWorkflow.Application.Ports.Inbound.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.Enums;
using EnterpriseWorkflow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
public interface IWorkflowEngineAdapter
{
    string EngineType { get; }
    Task<string> RegisterDefinitionAsync(Workflowdefinition definition, ExecutionModel ctx, CancellationToken cancellationToken = default);
    //Task UpdateDefinitionAsync(Workflowdefinition definition, ExecutionModel ctx, CancellationToken cancellationToken = default);
    //Task UnregisterDefinitionAsync(string definitionId, int version, ExecutionModel ctx, CancellationToken cancellationToken = default);
    //Task<WorkflowStartResult> StartWorkflowAsync(string definitionId, string? version, object input, ExecutionModel ctx, CancellationToken cancellationToken = default);
    //Task ResumeWorkflowAsync(string executionId, string signal, object? payload, ExecutionModel ctx, CancellationToken cancellationToken = default);
    //Task CancelWorkflowAsync(string executionId, string reason, ExecutionModel ctx, CancellationToken cancellationToken = default);
    //Task<WorkflowExecutionState> GetExecutionStateAsync(string executionId, CancellationToken cancellationToken = default);
    //Task<PagedResult<WorkflowExecutionSummary>> ListExecutionsAsync(string definitionId, WorkflowStatusFilter? status = null, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ValidationResult> ValidateDefinitionAsync(Workflowdefinition definition, CancellationToken cancellationToken = default);
    //Task<EngineHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
   // Task<ApproveWorkflowResponse> ApproveWorkflowAsync(WorkflowAprroveRequest approveWorkflow, ExecutionModel ctx, CancellationToken cancellationToken = default);
    Task<WorkflowStartResult> StartWorkflowAsync(WorkflowExecution definition, string? version, ExecutionModel ctx, CancellationToken cancellationToken = default);
    Task<DispatchWorkflowResponse> DispatchWorkflowAsync(WorkflowExecution definition, ExecutionModel ctx, CancellationToken cancellationToken = default);
    Task<CancelWorkflowResponse> CancelWorkflowAsync(WorkflowCancelRequest cancelWorkflow, ExecutionModel ctx, CancellationToken cancellationToken = default);
    Task<ResumeWorkflowResponse> ResumeWorkflowAsync(WorkflowResumeRequest resumeWorkflow, ExecutionModel ctx, CancellationToken cancellationToken = default);

    Task<WorkflowExecutionState> GetExecutionStateAsync(string executionId, CancellationToken cancellationToken = default);
    Task<WorkflowDefinitionPagedResponse> GetWorkflowDefinitionsAsync( WorkflowDefinitionQueryRequest request, ExecutionModel ctx, CancellationToken cancellationToken = default);

    Task<WorkflowDefinitionDetailPagedResponse> GetWorkflowDefinitionsDetailAsync(WorkflowDefinitionQueryRequest request, ExecutionModel ctx, CancellationToken cancellationToken = default);
    Task<ActivityExecutionBriefPagedResponse> GetActivityExecutionsAsync(WorkflowDefinitionQueryRequest request, string ExecutionId,ExecutionModel ctx,CancellationToken cancellationToken = default);
    Task<ActivityExecutionDetailPagedResponse> GetActivityExecutionsFullAsync(WorkflowDefinitionQueryRequest request,string executionId,ExecutionModel ctx,CancellationToken cancellationToken = default);
    Task<LastExecutedWorkflowActivityResponse> GetLastExecutedWorkflowActivityAsync(WorkflowDefinitionQueryRequest request, string DefinitionId, ExecutionModel ctx, CancellationToken cancellationToken = default);
    Task<LastExecutedWorkflowActivityDetailResponse> GetLastExecutedWorkflowActivityDetailAsync(WorkflowDefinitionQueryRequest request, string DefinitionId, ExecutionModel ctx, CancellationToken cancellationToken = default);
    Task<string> CreateDefinitionAsync(string json,CancellationToken ct = default);
    Task WaitForCompletionAsync(string workflowInstanceId,CancellationToken cancellationToken = default);
    Task<WorkflowStartResult> ExecuteDefinitionWithParentIdAsync(string definitionId, string parentWorkflowId, Dictionary<string, object>? input = null, CancellationToken ct = default);

    Task<string> LoadAndCreateAsync(string fileName, CancellationToken ct = default);
}


