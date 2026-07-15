using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Domain.Entities.Workflow;

//public class WorkflowExecution
//{
//    public string DefinitionId { get; set; }
//    public string Version { get; set; }
//    public Dictionary<string, object>? Input { get; set; }
    
//}
public class WorkflowAprroveRequest
{
    public long WorkflowversionId { get; set; }
}
public class WorkflowCancelRequest
{
    public string workflowExecutionId { get; set; }
    public string? signal { get; set; }
    public object? payload { get; set; }
}
public class WorkflowResumeRequest {
    public string workflowExecutionId { get; set; }
    public string Task {  get; set; }
    public Dictionary<string, object>? Payload { get; set; }

    public string Action { get; set; }
}
public class WorkflowDefinitionQueryRequest
{
    // ── Optional filters — key = column name, value = filter value
    public Dictionary<string, string>? Filters { get; set; }

    // ── Optional sorting
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    // ── Optional pagination
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

}
public class WorkflowDto
{
    public InputModel? Input { get; set; }
}

public class InputModel
{
    public string? entityId { get; set; }
    public string? entityType { get; set; }
    public bool clinicalConcern { get; set; }
    public bool claimAnomaly { get; set; }
    public bool providerBehaviourConcern { get; set; }
    public string? signal { get; set; }
}


public class NodeInstance
{
    public long Node_Instance_ID { get; set; }
    public long Workflow_Instance_ID { get; set; }
    public string Node_Type_Value { get; set; } = default!;
    public long Config_Node_ID { get; set; }
    public string Node_Instance_Number { get; set; } = default!;
    public string Node_Instance_Status_Value { get; set; } = default!;
    public string Node_Instance_System_Status_Value { get; set; } = default!;
    public string Node_Instance_Business_Status_Value { get; set; } = default!;
    public DateTime? Start_Date_Time { get; set; }
    public DateTime? Completed_Date_Time { get; set; }
    public string? Failure_Reason { get; set; }
    public int Created_By { get; set; }
    public int Updated_By { get; set; }
    public short Status_Code { get; set; }
}

public class NodeExecution
{
    public long Node_Execution_ID { get; set; }
    public long Node_Instance_ID { get; set; }
    public long Workflow_Execution_ID { get; set; }
    public string Node_Execution_Number { get; set; } = default!;
    public int Execution_Sequence_Number { get; set; }
    public string Execution_Type_Code { get; set; } = default!;
    public string Node_Execution_Status_Value { get; set; } = default!;
    public string Node_Execution_System_Status_Value { get; set; } = default!;
    public string Node_Execution_Business_Status_Value { get; set; } = default!;
    public DateTime? Start_Date_Time { get; set; }
    public DateTime? End_Date_Time { get; set; }
    public string? Failure_Reason { get; set; }
    public int Created_By { get; set; }
    public int Updated_By { get; set; }
    public short Status_Code { get; set; }
}

public class WorkflowExecution
{
    public long Workflow_Execution_ID { get; set; }
    public long Workflow_Instance_ID { get; set; }
    public string Workflow_Execution_Number { get; set; } = default!;
    public int Execution_Sequence_Number { get; set; }
    public string Execution_Type_Code { get; set; } = default!;
    public string Workflow_Execution_Status_Value { get; set; } = default!;
    public string Workflow_Execution_System_Status_Value { get; set; } = default!;
    public string Workflow_Execution_Business_Status_Value { get; set; } = default!;
    public DateTime? Start_Date_Time { get; set; }
    public DateTime? End_Date_Time { get; set; }
    public string? Failure_Reason { get; set; }
    public int Created_By { get; set; }
    public int Updated_By { get; set; }
    public short Status_Code { get; set; }
}

public class WorkflowInstance
{
    public long Workflow_Instance_ID { get; set; }
    public long? Parent_Workflow_Instance_ID { get; set; }
    public long Workflow_Version_ID { get; set; }
    public int Org_ID { get; set; }
    public long Domain_ID { get; set; }
    public string Workflow_Instance_Number { get; set; } = default!;
    public string Business_Reference_Type_Code { get; set; } = default!;
    public long Business_Reference_ID { get; set; }
    public string? Business_Reference_Number { get; set; }
    public string Workflow_Instance_Status_Value { get; set; } = default!;
    public string Workflow_Instance_System_Status_Value { get; set; } = default!;
    public string Workflow_Instance_Business_Status_Value { get; set; } = default!;
    public DateTime? Start_Date_Time { get; set; }
    public DateTime? Completed_Date_Time { get; set; }
    public DateTime? Cancelled_Date_Time { get; set; }
    public string? Failure_Reason { get; set; }
    public int Created_By { get; set; }
    public int Updated_By { get; set; }
    public short Status_Code { get; set; }
}

public class StatusHistory
{
    public long Status_History_ID { get; set; }
    public long Workflow_Instance_ID { get; set; }
    public long? Workflow_Execution_ID { get; set; }
    public string Status_Object_Type_Code { get; set; } = default!;
    public long Status_Object_ID { get; set; }
    public string Status_History_Number { get; set; } = default!;
    public int Status_Sequence_Number { get; set; }
    public string? Previous_Status_Value { get; set; }
    public string New_Status_Value { get; set; } = default!;
    public string? Previous_System_Status_Value { get; set; }
    public string New_System_Status_Value { get; set; } = default!;
    public string? Previous_Business_Status_Value { get; set; }
    public string New_Business_Status_Value { get; set; } = default!;
    public string Status_Change_Type_Code { get; set; } = default!;
    public string? Status_Change_Reason_Code { get; set; }
    public string? Status_Change_Description { get; set; }
    public string Changed_By_Source_Type_Code { get; set; } = default!;
    public DateTime Status_Changed_Date_Time { get; set; }
    public int Created_By { get; set; }
    public int Updated_By { get; set; }
    public short Status_Code { get; set; }
}

public class WorkflowWaitState
{
    public long Wait_State_ID { get; set; }
    public long Workflow_Instance_ID { get; set; }
    public long Workflow_Execution_ID { get; set; }
    public long? Node_Instance_ID { get; set; }
    public long? Node_Execution_ID { get; set; }
    public string Wait_State_Number { get; set; } = default!;
    public int Wait_Sequence_Number { get; set; }
    public string Wait_Type_Code { get; set; } = default!;
    public string? Wait_Reason_Code { get; set; }
    public string? Expected_Event_Name_Code { get; set; }
    public string? Wait_Correlation_Key { get; set; }
    public string Wait_State_Status_Value { get; set; } = default!;
    public string Wait_State_System_Status_Value { get; set; } = default!;
    public string Wait_State_Business_Status_Value { get; set; } = default!;
    public DateTime? Wait_Start_Date_Time { get; set; }
    public DateTime? Resumed_Date_Time { get; set; }
    public int Created_By { get; set; }
    public int Updated_By { get; set; }
    public short Status_Code { get; set; }
}

public record CurrentStatusDto(
    string Status,
    string SystemStatus,
    string BusinessStatus);