using EnterpriseWorkflow.Application.Model;
using EnterpriseWorkflow.Application.Ports.Inbound.Interface;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.Enums;
using EnterpriseWorkflow.Domain.ValueObjects;
using EnterpriseWorkflow.Utilities.Observability;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Traces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Services;

public class WorkflowExecutionService : IWorkflowExecution
{
    private readonly IWorkflowInstanceRepository _instanceRepo;
    private readonly IWorkflowExecutionRepository _executionRepo;
    private readonly IStatusHistoryRepository _statusHistoryRepo;
    private readonly IElsaWorkflowStarter _elsaStarter;

    public WorkflowExecutionService(
        IWorkflowInstanceRepository instanceRepo,
        IWorkflowExecutionRepository executionRepo,
        IStatusHistoryRepository statusHistoryRepo,
        IElsaWorkflowStarter elsaStarter)
    {
        _instanceRepo = instanceRepo;
        _executionRepo = executionRepo;
        _statusHistoryRepo = statusHistoryRepo;
        _elsaStarter = elsaStarter;
    }

    public async Task<StartWorkflowResponse> StartWorkflowAsync(StartWorkflowRequest request, CancellationToken ct)
    {
        var instanceNumber = Guid.NewGuid().ToString("N");

        var instance = new WorkflowInstance
        {
            Workflow_Version_ID = request.WorkflowVersionId,
            Org_ID = request.OrgId,
            Domain_ID = request.DomainId,
            Workflow_Instance_Number = instanceNumber,
            Business_Reference_Type_Code = request.BusinessRefType,
            Business_Reference_ID = request.BusinessRefId,
            Business_Reference_Number = request.BusinessRefNumber,
            Workflow_Instance_Status_Value = "Pending",
            Workflow_Instance_System_Status_Value = "PENDING",
            Workflow_Instance_Business_Status_Value = "Submitted",
            Start_Date_Time = DateTime.UtcNow,
            Created_By = request.UserId,
            Updated_By = request.UserId,
            Status_Code = 1
        };

        var instanceId = await _instanceRepo.InsertAsync(instance, ct);

        var execution = new WorkflowExecution
        {
            Workflow_Instance_ID = instanceId,
            Workflow_Execution_Number = Guid.NewGuid().ToString("N"),
            Execution_Sequence_Number = 1,
            Execution_Type_Code = "INITIAL",
            Workflow_Execution_Status_Value = "Pending",
            Workflow_Execution_System_Status_Value = "PENDING",
            Workflow_Execution_Business_Status_Value = "Queued",
            Created_By = request.UserId,
            Updated_By = request.UserId,
            Status_Code = 1
        };

        var executionId = await _executionRepo.InsertAsync(execution, ct);

        await _statusHistoryRepo.InsertAsync(new StatusHistory
        {
            Workflow_Instance_ID = instanceId,
            Workflow_Execution_ID = executionId,
            Status_Object_Type_Code = "WORKFLOW_INSTANCE",
            Status_Object_ID = instanceId,
            Status_History_Number = Guid.NewGuid().ToString("N"),
            Status_Sequence_Number = 1,
            New_Status_Value = "Pending",
            New_System_Status_Value = "PENDING",
            New_Business_Status_Value = "Submitted",
            Status_Change_Type_Code = "CREATED",
            Changed_By_Source_Type_Code = "API",
            Status_Changed_Date_Time = DateTime.UtcNow,
            Created_By = request.UserId,
            Updated_By = request.UserId,
            Status_Code = 1
        }, ct);

        // Build Elsa input: enterprise correlation IDs + caller's business payload merged together
        var elsaInput = new Dictionary<string, object>
        { 
            ["EnterpriseInstanceId"] = instanceId,
            ["EnterpriseExecutionId"] = executionId,
            ["InstanceNumber"] = instanceNumber  // Add this
        };
        foreach (var kvp in request.Payload)
        {
            if (kvp.Value is not null)
                elsaInput[kvp.Key] = kvp.Value;
        }

        var startResult = await _elsaStarter.StartAsync(
            request.ElsaWorkflowDefinitionId,
            correlationId: instanceNumber,
            input: elsaInput,
            ct: ct);

        if (!startResult.Success)
        {
            await _instanceRepo.UpdateStatusAsync(
                instanceId, "Rejected", "FAILED", "RejectedAtDispatch", ct,
                failureReason: startResult.ErrorMessage);

            await _executionRepo.UpdateStatusAsync(
                executionId, "Rejected", "FAILED", "RejectedAtDispatch",
                endDateTime: DateTime.UtcNow, failureReason: startResult.ErrorMessage, ct: ct);

            return new StartWorkflowResponse
            {
                WorkflowInstanceId = instanceId,
                WorkflowInstanceNumber = instanceNumber,
                WorkflowExecutionId = executionId,
                Status = "Rejected",
                ErrorMessage = startResult.ErrorMessage
            };
        }

        await _instanceRepo.UpdateEngineWorkflowInstanceAsync(
             startResult.ElsaWorkflowInstanceId, instanceNumber,ct, startResult.Status);


        return new StartWorkflowResponse
        {
            WorkflowInstanceId = instanceId,
            WorkflowInstanceNumber = instanceNumber,
            WorkflowExecutionId = executionId,
            Status = startResult.Status
        };
    }
}
