using EnterpriseWorkflow.Application.Model;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Inbound.Interface;

public interface IWorkflowExecution
{
   // Task ExecuteDefinitionWithParentIdAsync(string definitionId, string parentWorkflowInstanceId);
    Task<StartWorkflowResponse> StartWorkflowAsync(StartWorkflowRequest request, CancellationToken ct);
    //Task<WorkflowStartResult> ExecutionAsync(WorkflowExecution workflowStartRequest, ExecutionModel ctx, CancellationToken ct = default);
    //Task<ApproveWorkflowResponse> ApproveWorkflowAsync(WorkflowAprroveRequest approveWorkflow, ExecutionModel ctx, CancellationToken ct = default);
    //Task<CancelWorkflowResponse> CancelWorkflowAsync(WorkflowCancelRequest workflowCancel, ExecutionModel ctx, CancellationToken ct = default);
    //Task<ResumeWorkflowResponse> ResumeWorkflowAsync(WorkflowResumeRequest workflowResume, ExecutionModel ctx, CancellationToken ct = default);
    //Task<DispatchWorkflowResponse> DispatchWorkflowAsync(WorkflowExecution workflowStartRequest, ExecutionModel ctx, CancellationToken ct = default);

    //Task WaitForCompletionAsync(string workflowInstanceId, CancellationToken cancellationToken = default);

    //Task<WorkflowStartResult> ExecuteDefinitionWithParentIdAsync(string definitionId, string parentWorkflowId,  Dictionary<string, object>? input = null, CancellationToken ct = default);

}
