using Elsa.Mediator.Contracts;
using Elsa.Workflows;
using Elsa.Workflows.Notifications;
using Elsa.Workflows.Runtime.Notifications;   // For WorkflowFaulted, WorkflowExecutionSuspended
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;   // REMOVE THIS LINE – causes ambiguity
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Elsa.Extensions;
using EnterpriseWorkflow.Storage.Contracts;   // Keep only this one for repositories
using EnterpriseWorkflow.Utilities.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Elsa.Notifications;

public class ElsaNotificationHandler :
    INotificationHandler<ActivityExecuting>,
    INotificationHandler<ActivityExecuted>,
    INotificationHandler<WorkflowExecuted>,
     INotificationHandler<WorkflowExecuting>
    //INotificationHandler<WorkflowFaulted>,
    //INotificationHandler<WorkflowExecutionSuspended>
{
    private readonly INodeInstanceRepository _nodeInstanceRepo;
    private readonly INodeExecutionRepository _nodeExecutionRepo;
    private readonly IWorkflowExecutionRepository _executionRepo;
    private readonly IWorkflowInstanceRepository _instanceRepo;
    private readonly IStatusHistoryRepository _statusHistoryRepo;
    private readonly IWaitStateRepository _waitStateRepo;
    private readonly ILoggingService _logger;
    private readonly IConfigNodeLookupRepository _configNodeLookup;

    private static readonly Dictionary<string, string> NodeTypeMapping =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Workflow"] = "Workflow",
            ["Flowchart"] = "Workflow",
            ["ExecuteWorkflow"] = "SubWorkFlow",
            ["RunJavaScript"] = "Action",
            ["WriteLine"] = "Action",
            ["FlowJoin"] = "Action",
            ["SetVariable"] = "Action",
            ["SetOutput"] = "Action",
            ["Delay"] = "Action",
            ["FlowFork"] = "Step",
            ["SendHttpRequest"] = "Step",
            ["FlowDecision"] = "Step",
            ["EvaluateRuleActivity"] = "Rule",
            ["Event"] = "Task",
            ["HttpEndpoint"] = "Task",
            ["Cron"] = "Stage"
        };

    private static readonly Dictionary<string, (string Sys, string Biz)> StatusMap =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Running"] = ("RUNNING", "InProgress"),
            ["Completed"] = ("COMPLETED", "Completed"),
            ["Faulted"] = ("FAILED", "Failed"),
            ["Suspended"] = ("SUSPENDED", "WaitingForInput"),
            ["Cancelled"] = ("CANCELLED", "Cancelled")
        };

    public ElsaNotificationHandler(
        INodeInstanceRepository nodeInstanceRepo,
        INodeExecutionRepository nodeExecutionRepo,
        IWorkflowExecutionRepository executionRepo,
        IWorkflowInstanceRepository instanceRepo,
        IStatusHistoryRepository statusHistoryRepo,
        IWaitStateRepository waitStateRepo,
        IConfigNodeLookupRepository configNodeLookup,
        ILoggingService logger)
    {
        _nodeInstanceRepo = nodeInstanceRepo;
        _nodeExecutionRepo = nodeExecutionRepo;
        _executionRepo = executionRepo;
        _instanceRepo = instanceRepo;
        _statusHistoryRepo = statusHistoryRepo;
        _waitStateRepo = waitStateRepo;
        _configNodeLookup = configNodeLookup;
        _logger = logger;
    }

    // ----------------------------------------------------------------
    // Node-level (per activity)
    // ----------------------------------------------------------------

    public async Task HandleAsync(ActivityExecuting notification, CancellationToken ct)
    {
        var ctx = notification.ActivityExecutionContext;
        _logger.LogInformation("ActivityExecuting", $"Starting activity {ctx.Activity.Id}");

                try
        {
            var enterpriseInstanceId = ctx.GetEnterpriseInstanceId();
            var enterpriseExecutionId = ctx.GetEnterpriseExecutionId();

       

            if (enterpriseInstanceId == 0)
            {
                _logger.LogWarning("ActivityExecuting",
                    $"No EnterpriseInstanceId on workflow {ctx.WorkflowExecutionContext.Id} — skipping enterprise log.");
                return;
            }

            ctx.Properties["EnterpriseInstanceId"] = enterpriseInstanceId;
            ctx.Properties["EnterpriseExecutionId"] = enterpriseExecutionId;


            // Also store instance number
            var instanceNumber = ctx.GetInstanceNumber();
            if (!string.IsNullOrEmpty(instanceNumber))
            {
                ctx.Properties["InstanceNumber"] = instanceNumber;
            }

            // Update the database with Elsa Instance ID - do this only once per workflow
            var elsaInstanceId = ctx.WorkflowExecutionContext.Id;

            // Use a flag to ensure we only update once
            if (!ctx.WorkflowExecutionContext.Properties.ContainsKey("ElsaInstanceIdUpdated"))
            {
                await _instanceRepo.UpdateEngineWorkflowInstanceAsync(
                    elsaInstanceId,
                    instanceNumber ?? ctx.WorkflowExecutionContext.CorrelationId ?? string.Empty,
                    ct,
                    "Running");

                ctx.WorkflowExecutionContext.Properties["ElsaInstanceIdUpdated"] = true;

                _logger.LogInformation("ActivityExecuting",
                    $"Updated enterprise instance {enterpriseInstanceId} with Elsa instance ID {elsaInstanceId}");
            }
            // KEY FIX: look up by the actual Elsa activity id, not a hardcoded 0
            var configNode = await _configNodeLookup
                .GetByEngineActivityReferenceAsync(ctx.Activity.Id, ct);


            if (configNode is null)
            {
                // Log it — but don't throw. Unknown activities (e.g. framework-internal ones)
                // should not break the workflow.
                _logger.LogWarning("ActivityExecuting",
                    $"No enterprise config node found for Elsa activity '{ctx.Activity.Id}' " +
                    $"(type={ctx.Activity.GetType().Name}). " +
                    $"Enterprise status will NOT be updated for this activity. " +
                    $"Add Engine_Activity_Reference='{ctx.Activity.Id}' to the correct config table.");
                return;
            }

            var nodeInstanceNumber = $"{ctx.WorkflowExecutionContext.Id}:{ctx.Activity.Id}";
            var nodeType = GetNodeType(ctx.Activity);

            var nodeInstance = await _nodeInstanceRepo.GetOrCreateAsync(
                workflowInstanceId: enterpriseInstanceId,
                configNodeId: configNode.ConfigNodeId,
                nodeTypeValue: nodeType,
                nodeInstanceNumber: nodeInstanceNumber,
                ct: ct);

            var sequence = await _nodeExecutionRepo.NextSequenceAsync(nodeInstance.Node_Instance_ID, ct);

            await _nodeExecutionRepo.InsertAsync(new NodeExecution
            {
                Node_Instance_ID = nodeInstance.Node_Instance_ID,
                Workflow_Execution_ID = enterpriseExecutionId,
                Node_Execution_Number = Guid.NewGuid().ToString("N"),
                Execution_Sequence_Number = sequence,
                Execution_Type_Code = "NORMAL",
                Node_Execution_Status_Value = "Running",
                Node_Execution_System_Status_Value = "RUNNING",
                Node_Execution_Business_Status_Value = "InProgress",
                Start_Date_Time = DateTime.UtcNow,
                Created_By = 1,
                Updated_By = 1,
                Status_Code = 1
            }, ct);


           


        }
        catch (Exception ex)
        {
            _logger.LogError("ActivityExecuting",
                $"Failed to log activity start for '{ctx.Activity.Id}': {ex.Message}");
        }
    }

    public async Task HandleAsync(ActivityExecuted notification, CancellationToken ct)
    {
        var ctx = notification.ActivityExecutionContext;
        var status = ctx.Status.ToString();
        _logger.LogInformation("ActivityExecuted", $"Activity {ctx.Activity.Id} finished with status {status}");

        try
        {
            var (sys, biz) = StatusMap.GetValueOrDefault(status, ("UNKNOWN", "Unknown"));
            var nodeInstanceNumber = $"{ctx.WorkflowExecutionContext.Id}:{ctx.Activity.Id}";

            await _nodeExecutionRepo.UpdateLatestAsync(
                nodeInstanceNumber: nodeInstanceNumber,
                newStatusValue: status,
                newSystemStatus: sys,
                newBusinessStatus: biz,
                endDateTime: DateTime.UtcNow,
                failureReason: ctx.Exception?.Message,
                ct: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError("ActivityExecuted", $"Failed to log activity end for {ctx.Activity.Id}: {ex.Message}");
        }
    }

    // ----------------------------------------------------------------
    // Workflow-level
    // ----------------------------------------------------------------

    public async Task HandleAsync(WorkflowExecuted notification, CancellationToken ct)
    {
        _logger.LogInformation("WorkflowExecuted", $"Workflow {notification.WorkflowExecutionContext.Id} completed");

        var ctx = notification.WorkflowExecutionContext;

        _logger.LogInformation(
            "WorkflowExecuted",
            $"Status={ctx.Status}, SubStatus={ctx.SubStatus}");

        await UpdateInstanceStatusAsync(ctx, ctx.Status.ToString(), ctx.SubStatus.ToString(), ct);

 
       


    }



    //public async Task HandleAsync(WorkflowFaulted notification, CancellationToken ct)
    //{
    //    var ex = notification.WorkflowExecutionContext.GetFault()?.Exception;
    //    _logger.LogError("WorkflowFaulted",
    //        $"Workflow {notification.WorkflowExecutionContext.Id} faulted: {ex?.Message}");

    //    await UpdateInstanceStatusAsync(notification.WorkflowExecutionContext, "Faulted", ct, ex);
    //}

    //public async Task HandleAsync(WorkflowExecutionSuspended notification, CancellationToken ct)
    //{
    //    var ctx = notification.WorkflowExecutionContext;
    //    _logger.LogInformation("WorkflowExecutionSuspended", $"Workflow {ctx.Id} suspended (bookmark created)");

    //    try
    //    {
    //        var enterpriseInstanceId = ctx.GetEnterpriseInstanceId();
    //        var enterpriseExecutionId = ctx.GetEnterpriseExecutionId();

    //        if (enterpriseInstanceId == 0) return;

    //        await UpdateInstanceStatusAsync(ctx, "Suspended", ct);

    //        await _waitStateRepo.InsertAsync(new WorkflowWaitState
    //        {
    //            Workflow_Instance_ID = enterpriseInstanceId,
    //            Workflow_Execution_ID = enterpriseExecutionId,
    //            Wait_State_Number = Guid.NewGuid().ToString("N"),
    //            Wait_Sequence_Number = 1,
    //            Wait_Type_Code = "BOOKMARK",
    //            Wait_Reason_Code = "ELSA_SUSPENSION",
    //            Wait_Correlation_Key = ctx.CorrelationId,
    //            Wait_State_Status_Value = "Waiting",
    //            Wait_State_System_Status_Value = "WAITING",
    //            Wait_State_Business_Status_Value = "WaitingForInput",
    //            Wait_Start_Date_Time = DateTime.UtcNow,
    //            Created_By = 0,
    //            Updated_By = 0,
    //            Status_Code = 1
    //        }, ct);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError("WorkflowExecutionSuspended", $"Failed to log suspension for {ctx.Id}: {ex.Message}");
    //    }
    //}

    private async Task UpdateInstanceWaitState(
    WorkflowExecutionContext ctx, string elsaStatus,Int64 enterpriseInstanceId, Int64 enterpriseExecutionId, CancellationToken ct, Exception? ex = null)
    {

        try
        {
           

            if (enterpriseInstanceId == 0) return;

           // await UpdateInstanceStatusAsync(ctx, "Suspended", ct);

            await _waitStateRepo.InsertAsync(new WorkflowWaitState
            {
                Workflow_Instance_ID = enterpriseInstanceId,
                Workflow_Execution_ID = enterpriseExecutionId,
                Wait_State_Number = Guid.NewGuid().ToString("N"),
                Wait_Sequence_Number = 1,
                Wait_Type_Code = "BOOKMARK",
                Wait_Reason_Code = "ELSA_SUSPENSION",
                Wait_Correlation_Key = ctx.CorrelationId,
                Wait_State_Status_Value = "Waiting",
                Wait_State_System_Status_Value = "WAITING",
                Wait_State_Business_Status_Value = "WaitingForInput",
                Wait_Start_Date_Time = DateTime.UtcNow,
                Created_By = 1,
                Updated_By = 1,
                Status_Code = 1
            }, ct);
        }
        catch (Exception e)
        {
            _logger.LogError("WorkflowExecutionSuspended", $"Failed to log suspension for {ctx.Id}: {e.Message}");
        }
    }

        // ----------------------------------------------------------------
        // Shared status-update logic
        // ----------------------------------------------------------------
    public async Task HandleAsync(WorkflowExecuting notification, CancellationToken ct)
    {
        var ctx = notification.WorkflowExecutionContext;
        _logger.LogInformation("WorkflowExecuting", $"Workflow {ctx.Id} starting");

        try
        {
            // The workflow is just starting - now we have the Elsa Instance ID available
            var elsaInstanceId = ctx.Id;

            // Get the enterprise IDs from input (these were set when workflow was dispatched)
            var enterpriseInstanceId = ctx.GetEnterpriseInstanceId();
            var enterpriseExecutionId = ctx.GetEnterpriseExecutionId();

            if (enterpriseInstanceId == 0)
            {
                _logger.LogWarning("WorkflowExecuting",
                    $"No EnterpriseInstanceId on workflow {elsaInstanceId} — skipping enterprise update.");
               // return;
            }

            // NOW we can update the database with the Elsa instance ID
            var instanceNumber = ctx.GetInstanceNumber(); // This was set as correlationId when starting the workflow

            await _instanceRepo.UpdateEngineWorkflowInstanceAsync(
                elsaInstanceId,
                instanceNumber,
                ct,
                "Running");

            _logger.LogInformation("WorkflowExecuting",
                $"Updated enterprise instance {enterpriseInstanceId} with Elsa instance ID {elsaInstanceId}");


        }
        catch (Exception ex)
        {
            _logger.LogError("WorkflowExecuting",
                $"Failed to update enterprise with Elsa instance ID for workflow {ctx.Id}: {ex.Message}");
        }
    }

    private async Task UpdateInstanceStatusAsync(
        WorkflowExecutionContext ctx, string elsaStatus, string elsaSubStatus, CancellationToken ct, Exception? ex = null)
    {

        if (elsaSubStatus == "Suspended")
        {
            // If the workflow is not suspended, we can remove any existing wait state
            var enterpriseInstanceId = ctx.GetEnterpriseInstanceId();
            var enterpriseExecutionId = ctx.GetEnterpriseExecutionId();
           
            await UpdateInstanceWaitState(ctx, elsaSubStatus.ToString(), enterpriseInstanceId, enterpriseExecutionId, ct);
        }   
        try
        {
            var enterpriseInstanceId = ctx.GetEnterpriseInstanceId();
            var enterpriseExecutionId = ctx.GetEnterpriseExecutionId();

            if (enterpriseInstanceId == 0)
            {
                _logger.LogError("UpdateInstanceStatus",
                    $"No EnterpriseInstanceId on workflow {ctx.Id}. Cannot update enterprise status.");
                //  return;

                    // Try fallback to database lookup only if IDs aren't in context
                    var EnterpriseInstanceRecord = await _instanceRepo
                        .GetByInstanceNumberUsingEngineInstanceNumberAsync(ctx.ParentWorkflowInstanceId == null ? ctx.Id : ctx.ParentWorkflowInstanceId, ct);
                    enterpriseInstanceId = EnterpriseInstanceRecord?.Workflow_Instance_ID ?? 0;

                    if (enterpriseInstanceId == 0)
                    {
                        _logger.LogError("UpdateInstanceStatus",
                            $"Cannot find enterprise instance for Elsa  {ctx.Id}");
                        return;
                    }

                    var EnterpriseExecutionRecord = await _executionRepo
                        .GetByExecutionNoUsingEnterpriseInstanceNumberAsync(enterpriseInstanceId, ct);
                    enterpriseExecutionId = EnterpriseExecutionRecord?.Workflow_Execution_ID ?? 0;
                }

                var (sys, biz) = StatusMap.GetValueOrDefault(elsaStatus, ("UNKNOWN", "Unknown"));
            var previous = await _instanceRepo.GetCurrentStatusAsync(enterpriseInstanceId, ct);

            if (IsTerminal(previous.SystemStatus))
            {
                _logger.LogInformation("UpdateInstanceStatus",
                    $"Instance {enterpriseInstanceId} already terminal ({previous.SystemStatus}); ignoring {elsaStatus}.");
                return;
            }

            await _instanceRepo.UpdateStatusAsync(
                enterpriseInstanceId, elsaStatus, sys, biz, ct, failureReason: ex?.Message);

            await _executionRepo.UpdateStatusAsync(
                enterpriseExecutionId,
                elsaStatus,
                sys,
                biz,
                endDateTime: elsaStatus is "Completed" or "Faulted" or "Cancelled" ? DateTime.UtcNow : (DateTime?)null,
                failureReason: ex?.Message,
                ct: ct);

            var sequence = await _statusHistoryRepo.NextSequenceAsync(enterpriseInstanceId, ct);

            await _statusHistoryRepo.InsertAsync(new StatusHistory
            {
                Workflow_Instance_ID = enterpriseInstanceId,
                Workflow_Execution_ID = enterpriseExecutionId,
                Status_Object_Type_Code = "WORKFLOW_INSTANCE",
                Status_Object_ID = enterpriseInstanceId,
                Status_History_Number = Guid.NewGuid().ToString("N"),
                Status_Sequence_Number = sequence,
                Previous_Status_Value = previous.Status,
                New_Status_Value = elsaStatus,
                Previous_System_Status_Value = previous.SystemStatus,
                New_System_Status_Value = sys,
                Previous_Business_Status_Value = previous.BusinessStatus,
                New_Business_Status_Value = biz,
                Status_Change_Type_Code = "ENGINE_EVENT",
                Status_Change_Reason_Code = ex is not null ? "EXCEPTION" : "NORMAL",
                Status_Change_Description = ex is not null
                    ? JsonSerializer.Serialize(new { ex.Message, ex.StackTrace, Type = ex.GetType().FullName })
                    : null,
                Changed_By_Source_Type_Code = "ELSA",
                Status_Changed_Date_Time = DateTime.UtcNow,
                Created_By = 1,
                Updated_By = 1,
                Status_Code = 1
            }, ct);


            if (elsaSubStatus.ToString() == "Suspended")
            {
                await UpdateInstanceWaitState(ctx, elsaSubStatus.ToString(),enterpriseInstanceId, enterpriseExecutionId, ct);
            }
        }
        catch (Exception ex_obj)
        {
            _logger.LogError("UpdateInstanceStatus",$"Failed to update enterprise status for workflow {ctx.Id}: {ex_obj.Message}");
        }
    }

    private static bool IsTerminal(string status) =>
        status is "COMPLETED" or "FAILED" or "CANCELLED";

    private static string GetNodeType(IActivity activity)
    {
        var activityType = activity.GetType().Name;
        return NodeTypeMapping.TryGetValue(activityType, out var nodeType) ? nodeType : "Unknown";
    }

    private static long ResolveConfigNodeId(IActivity activity)
    {
        // TODO: replace with a real lookup against WF_CONFIG_Steps/Stages/Tasks/Actions
        // keyed by activity.Id or a custom metadata property you set on the Elsa activity.
        // Returning 0 for now as a placeholder so inserts don't fail on NOT NULL Config_Node_ID.
        return 0;
    }
}