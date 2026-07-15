using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Storage.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Storage.Implementations.Repositories;

public class WaitStateRepository : IWaitStateRepository
{

    private readonly string _connectionString;

    public WaitStateRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("EnterpriseDb");
    }
    public async Task<long> InsertAsync(WorkflowWaitState e, CancellationToken ct)
    {
        const string sql = @"
            INSERT INTO Workflow.WF_EXEC_Workflow_Wait_State
                (Workflow_Instance_ID, Workflow_Execution_ID, Node_Instance_ID, Node_Execution_ID,
                 Wait_State_Number, Wait_Sequence_Number, Wait_Type_Code, Wait_Reason_Code,
                 Expected_Event_Name_Code, Wait_Correlation_Key,
                 Wait_State_Status_Value, Wait_State_System_Status_Value, Wait_State_Business_Status_Value,
                 Wait_Start_Date_Time, Created_By, Updated_By, Status_Code)
            OUTPUT INSERTED.Wait_State_ID
            VALUES
                (@WorkflowInstanceId, @WorkflowExecutionId, @NodeInstanceId, @NodeExecutionId,
                 @WaitNumber, @SequenceNumber, @WaitTypeCode, @WaitReasonCode,
                 @ExpectedEventNameCode, @CorrelationKey,
                 @StatusValue, @SystemStatus, @BusinessStatus,
                 @WaitStartDateTime, @CreatedBy, @UpdatedBy, @StatusCode);";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@WorkflowInstanceId", e.Workflow_Instance_ID);
        cmd.Parameters.AddWithValue("@WorkflowExecutionId", e.Workflow_Execution_ID);
        cmd.Parameters.AddWithValue("@NodeInstanceId", e.Node_Instance_ID ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@NodeExecutionId", e.Node_Execution_ID ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@WaitNumber", e.Wait_State_Number);
        cmd.Parameters.AddWithValue("@SequenceNumber", e.Wait_Sequence_Number);
        cmd.Parameters.AddWithValue("@WaitTypeCode", e.Wait_Type_Code);
        cmd.Parameters.AddWithValue("@WaitReasonCode", e.Wait_Reason_Code ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@ExpectedEventNameCode", e.Expected_Event_Name_Code ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CorrelationKey", e.Wait_Correlation_Key ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@StatusValue", e.Wait_State_Status_Value);
        cmd.Parameters.AddWithValue("@SystemStatus", e.Wait_State_System_Status_Value);
        cmd.Parameters.AddWithValue("@BusinessStatus", e.Wait_State_Business_Status_Value);
        cmd.Parameters.AddWithValue("@WaitStartDateTime", e.Wait_Start_Date_Time ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedBy", e.Created_By);
        cmd.Parameters.AddWithValue("@UpdatedBy", e.Updated_By);
        cmd.Parameters.AddWithValue("@StatusCode", e.Status_Code);

        return (long)(await cmd.ExecuteScalarAsync(ct))!;
    }

    public async Task<WorkflowWaitState?> GetActiveByCorrelationKeyAsync(string correlationKey, CancellationToken ct)
    {
        const string sql = @"
            SELECT TOP 1 Wait_State_ID, Workflow_Instance_ID, Workflow_Execution_ID, Node_Instance_ID,
                   Node_Execution_ID, Wait_State_Number, Wait_Sequence_Number, Wait_Type_Code,
                   Wait_Reason_Code, Expected_Event_Name_Code, Wait_Correlation_Key,
                   Wait_State_Status_Value, Wait_State_System_Status_Value, Wait_State_Business_Status_Value,
                   Wait_Start_Date_Time, Resumed_Date_Time
            FROM Workflow.WF_EXEC_Workflow_Wait_State
            WHERE Wait_Correlation_Key = @CorrelationKey
              AND Wait_State_System_Status_Value = 'WAITING'
              AND Is_Active_Inactive_Deleted_Flag = 'A'
            ORDER BY Wait_Sequence_Number DESC;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@CorrelationKey", correlationKey);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        return new WorkflowWaitState
        {
            Wait_State_ID = reader.GetInt64(0),
            Workflow_Instance_ID = reader.GetInt64(1),
            Workflow_Execution_ID = reader.GetInt64(2),
            Node_Instance_ID = reader.IsDBNull(3) ? null : reader.GetInt64(3),
            Node_Execution_ID = reader.IsDBNull(4) ? null : reader.GetInt64(4),
            Wait_State_Number = reader.GetString(5),
            Wait_Sequence_Number = reader.GetInt32(6),
            Wait_Type_Code = reader.GetString(7),
            Wait_Reason_Code = reader.IsDBNull(8) ? null : reader.GetString(8),
            Expected_Event_Name_Code = reader.IsDBNull(9) ? null : reader.GetString(9),
            Wait_Correlation_Key = reader.IsDBNull(10) ? null : reader.GetString(10),
            Wait_State_Status_Value = reader.GetString(11),
            Wait_State_System_Status_Value = reader.GetString(12),
            Wait_State_Business_Status_Value = reader.GetString(13),
            Wait_Start_Date_Time = reader.IsDBNull(14) ? null : reader.GetDateTime(14),
            Resumed_Date_Time = reader.IsDBNull(15) ? null : reader.GetDateTime(15)
        };
    }

    public async Task MarkResumedAsync(long waitStateId, CancellationToken ct)
    {
        const string sql = @"
            UPDATE Workflow.WF_EXEC_Workflow_Wait_State
            SET Wait_State_Status_Value = 'Resumed',
                Wait_State_System_Status_Value = 'RESUMED',
                Wait_State_Business_Status_Value = 'Resumed',
                Resumed_Date_Time = SYSUTCDATETIME(),
                Updated_Date_Time = SYSUTCDATETIME()
            WHERE Wait_State_ID = @Id;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", waitStateId);

        await cmd.ExecuteNonQueryAsync(ct);
    }
}
