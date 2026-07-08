using EnterpriseWorkflow.Application.Model;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Inbound.Interface;

public interface IGetReportSummary
{
    Task<WorkflowDefinitionPagedResponse> GetWorkflowDefinitionsAsync(WorkflowDefinitionQueryRequest request, ExecutionModel ctx, CancellationToken ct = default);
    Task<WorkflowDefinitionDetailPagedResponse> GetWorkflowDefinitionsDetailAsync(WorkflowDefinitionQueryRequest request, ExecutionModel ctx,CancellationToken cancellationToken = default);
    Task<ActivityExecutionBriefPagedResponse> GetWorkflowInstanceActivityAsync(WorkflowDefinitionQueryRequest request,string ExecutionId, ExecutionModel ctx, CancellationToken cancellationToken = default);
    Task<ActivityExecutionDetailPagedResponse> GetWorkflowInstanceActivityDetailAsync(WorkflowDefinitionQueryRequest request,string ExecutionId,ExecutionModel ctx,CancellationToken cancellationToken = default);
    Task<LastExecutedWorkflowActivityResponse> GetLastExecutedWorkflowActivityAsync(WorkflowDefinitionQueryRequest request,string DefinitionId, ExecutionModel ctx,CancellationToken cancellationToken = default);
    Task<LastExecutedWorkflowActivityDetailResponse> GetLastExecutedWorkflowActivityDetailAsync(WorkflowDefinitionQueryRequest request, string DefinitionId, ExecutionModel ctx, CancellationToken cancellationToken = default);
}
