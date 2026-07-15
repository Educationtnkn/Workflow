using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Storage.Implementations.Repositories;

public class StatusHistoryRepository : IStatusHistoryRepository
{

    private readonly string _connectionString;

    public StatusHistoryRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("EnterpriseDb");
    }

    public async Task<long> InsertAsync(StatusHistory e, CancellationToken ct)
    {
        const string sql = @"
            INSERT INTO Workflow.WF_EXEC_Status_History
                (Workflow_Instance_ID, Workflow_Execution_ID, Status_Object_Type_Code, Status_Object_ID,
                 Status_History_Number, Status_Sequence_Number, Previous_Status_Value, New_Status_Value,
                 Previous_System_Status_Value, New_System_Status_Value,
                 Previous_Business_Status_Value, New_Business_Status_Value,
                 Status_Change_Type_Code, Status_Change_Reason_Code, Status_Change_Description,
                 Changed_By_Source_Type_Code, Status_Changed_Date_Time,
                 Created_By, Updated_By, Status_Code)
            OUTPUT INSERTED.Status_History_ID
            VALUES
                (@WorkflowInstanceId, @WorkflowExecutionId, @ObjectTypeCode, @ObjectId,
                 @HistoryNumber, @SequenceNumber, @PrevStatus, @NewStatus,
                 @PrevSystemStatus, @NewSystemStatus, @PrevBusinessStatus, @NewBusinessStatus,
                 @ChangeTypeCode, @ChangeReasonCode, @ChangeDescription,
                 @ChangedBySourceType, @ChangedDateTime, @CreatedBy, @UpdatedBy, @StatusCode);";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@WorkflowInstanceId", e.Workflow_Instance_ID);
        cmd.Parameters.AddWithValue("@WorkflowExecutionId", e.Workflow_Execution_ID ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ObjectTypeCode", e.Status_Object_Type_Code);
        cmd.Parameters.AddWithValue("@ObjectId", e.Status_Object_ID);
        cmd.Parameters.AddWithValue("@HistoryNumber", e.Status_History_Number);
        cmd.Parameters.AddWithValue("@SequenceNumber", e.Status_Sequence_Number);
        cmd.Parameters.AddWithValue("@PrevStatus", e.Previous_Status_Value ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@NewStatus", e.New_Status_Value);
        cmd.Parameters.AddWithValue("@PrevSystemStatus", e.Previous_System_Status_Value ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@NewSystemStatus", e.New_System_Status_Value);
        cmd.Parameters.AddWithValue("@PrevBusinessStatus", e.Previous_Business_Status_Value ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@NewBusinessStatus", e.New_Business_Status_Value);
        cmd.Parameters.AddWithValue("@ChangeTypeCode", e.Status_Change_Type_Code);
        cmd.Parameters.AddWithValue("@ChangeReasonCode", e.Status_Change_Reason_Code ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ChangeDescription", e.Status_Change_Description ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ChangedBySourceType", e.Changed_By_Source_Type_Code);
        cmd.Parameters.AddWithValue("@ChangedDateTime", e.Status_Changed_Date_Time);
        cmd.Parameters.AddWithValue("@CreatedBy", e.Created_By);
        cmd.Parameters.AddWithValue("@UpdatedBy", e.Updated_By);
        cmd.Parameters.AddWithValue("@StatusCode", e.Status_Code);

        return (long)(await cmd.ExecuteScalarAsync(ct))!;
    }

    public async Task<int> NextSequenceAsync(long workflowInstanceId, CancellationToken ct)
    {
        const string sql = @"
            SELECT ISNULL(MAX(Status_Sequence_Number), 0) + 1
            FROM Workflow.WF_EXEC_Status_History
            WHERE Workflow_Instance_ID = @Id;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", workflowInstanceId);

        return (int)(await cmd.ExecuteScalarAsync(ct))!;
    }
}
