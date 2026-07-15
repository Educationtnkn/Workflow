using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Storage.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Storage.Implementations.Repositories
{
    public class NodeExecutionRepository : INodeExecutionRepository
    {
        private readonly string _connectionString;

        public NodeExecutionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("EnterpriseDb");
        }
        public async Task<long> InsertAsync(NodeExecution e, CancellationToken ct)
        {
            const string sql = @"
            INSERT INTO Workflow.WF_EXEC_Node_Execution
                (Node_Instance_ID, Workflow_Execution_ID, Node_Execution_Number, Execution_Sequence_Number,
                 Execution_Type_Code, Node_Execution_Status_Value, Node_Execution_System_Status_Value,
                 Node_Execution_Business_Status_Value, Start_Date_Time, Created_By, Updated_By, Status_Code)
            OUTPUT INSERTED.Node_Execution_ID
            VALUES
                (@NodeInstanceId, @WorkflowExecutionId, @NodeExecutionNumber, @SequenceNumber,
                 @ExecutionTypeCode, @StatusValue, @SystemStatus, @BusinessStatus,
                 @StartDateTime, @CreatedBy, @UpdatedBy, @StatusCode);";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NodeInstanceId", e.Node_Instance_ID);
            cmd.Parameters.AddWithValue("@WorkflowExecutionId", e.Workflow_Execution_ID);
            cmd.Parameters.AddWithValue("@NodeExecutionNumber", e.Node_Execution_Number);
            cmd.Parameters.AddWithValue("@SequenceNumber", e.Execution_Sequence_Number);
            cmd.Parameters.AddWithValue("@ExecutionTypeCode", e.Execution_Type_Code);
            cmd.Parameters.AddWithValue("@StatusValue", e.Node_Execution_Status_Value);
            cmd.Parameters.AddWithValue("@SystemStatus", e.Node_Execution_System_Status_Value);
            cmd.Parameters.AddWithValue("@BusinessStatus", e.Node_Execution_Business_Status_Value);
            cmd.Parameters.AddWithValue("@StartDateTime", e.Start_Date_Time ?? (object)DBNull.
                Value);
            cmd.Parameters.AddWithValue("@CreatedBy", e.Created_By);
            cmd.Parameters.AddWithValue("@UpdatedBy", e.Updated_By);
            cmd.Parameters.AddWithValue("@StatusCode", e.Status_Code);

            return (long)(await cmd.ExecuteScalarAsync(ct))!;
        }

        public async Task<int> NextSequenceAsync(long nodeInstanceId, CancellationToken ct)
        {
            const string sql = @"
            SELECT ISNULL(MAX(Execution_Sequence_Number), 0) + 1
            FROM Workflow.WF_EXEC_Node_Execution
            WHERE Node_Instance_ID = @NodeInstanceId;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NodeInstanceId", nodeInstanceId);

            return (int)(await cmd.ExecuteScalarAsync(ct))!;
        }

        public async Task UpdateLatestAsync(
            string nodeInstanceNumber, string newStatusValue, string newSystemStatus,
            string newBusinessStatus, DateTime? endDateTime, string? failureReason, CancellationToken ct)
        {
            const string sql = @"
            UPDATE ne
            SET ne.Node_Execution_Status_Value = @Status,
                ne.Node_Execution_System_Status_Value = @SystemStatus,
                ne.Node_Execution_Business_Status_Value = @BusinessStatus,
                ne.End_Date_Time = @EndDateTime,
                ne.Failure_Reason = @FailureReason,
                ne.Updated_Date_Time = SYSUTCDATETIME()
            FROM Workflow.WF_EXEC_Node_Execution ne
            INNER JOIN Workflow.WF_EXEC_Node_Instance ni
                ON ni.Node_Instance_ID = ne.Node_Instance_ID
            WHERE ni.Node_Instance_Number = @NodeInstanceNumber
              AND ne.Node_Execution_ID = (
                    SELECT TOP 1 ne2.Node_Execution_ID
                    FROM Workflow.WF_EXEC_Node_Execution ne2
                    WHERE ne2.Node_Instance_ID = ni.Node_Instance_ID
                    ORDER BY ne2.Execution_Sequence_Number DESC);

            UPDATE Workflow.WF_EXEC_Node_Instance
            SET Node_Instance_Status_Value = @Status,
                Node_Instance_System_Status_Value = @SystemStatus,
                Node_Instance_Business_Status_Value = @BusinessStatus,
                Completed_Date_Time = @EndDateTime,
                Failure_Reason = @FailureReason,
                Updated_Date_Time = SYSUTCDATETIME()
            WHERE Node_Instance_Number = @NodeInstanceNumber;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NodeInstanceNumber", nodeInstanceNumber);
            cmd.Parameters.AddWithValue("@Status", newStatusValue);
            cmd.Parameters.AddWithValue("@SystemStatus", newSystemStatus);
            cmd.Parameters.AddWithValue("@BusinessStatus", newBusinessStatus);
            cmd.Parameters.AddWithValue("@EndDateTime", endDateTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@FailureReason", failureReason ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }

}
