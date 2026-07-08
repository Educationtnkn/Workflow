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
    public class NodeInstanceRepository : INodeInstanceRepository
    {
        private readonly string _connectionString;
        public NodeInstanceRepository(string connectionString) => _connectionString = connectionString;

        public NodeInstanceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("EnterpriseDb");
        }

        public async Task<NodeInstance> GetOrCreateAsync(
            long workflowInstanceId, long configNodeId, string nodeTypeValue,
            string nodeInstanceNumber, CancellationToken ct)
        {
            var existing = await GetByNumberAsync(nodeInstanceNumber, ct);
            if (existing is not null) return existing;

            const string sql = @"
            INSERT INTO Workflow.WF_EXEC_Node_Instance
                (Workflow_Instance_ID, Node_Type_Value, Config_Node_ID, Node_Instance_Number,
                 Node_Instance_Status_Value, Node_Instance_System_Status_Value, Node_Instance_Business_Status_Value,
                 Start_Date_Time, Created_By, Updated_By, Status_Code)
            OUTPUT INSERTED.Node_Instance_ID
            VALUES
                (@WorkflowInstanceId, @NodeTypeValue, @ConfigNodeId, @NodeInstanceNumber,
                 'Running', 'RUNNING', 'InProgress',
                 SYSUTCDATETIME(), 1, 1, 1);";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@WorkflowInstanceId", workflowInstanceId);
            cmd.Parameters.AddWithValue("@NodeTypeValue", nodeTypeValue);
            cmd.Parameters.AddWithValue("@ConfigNodeId", configNodeId);
            cmd.Parameters.AddWithValue("@NodeInstanceNumber", nodeInstanceNumber);

            //var newId = (long)(await cmd.ExecuteScalarAsync(ct))!;
            var result = await cmd.ExecuteScalarAsync(ct);
            var newId = Convert.ToInt64(result);

            return new NodeInstance
            {
                Node_Instance_ID = newId,
                Workflow_Instance_ID = workflowInstanceId,
                Node_Type_Value = nodeTypeValue,
                Config_Node_ID = configNodeId,
                Node_Instance_Number = nodeInstanceNumber,
                Node_Instance_Status_Value = "Running",
                Node_Instance_System_Status_Value = "RUNNING",
                Node_Instance_Business_Status_Value = "InProgress"
            };
        }

        public async Task<NodeInstance?> GetByNumberAsync(string nodeInstanceNumber, CancellationToken ct)
        {
            const string sql = @"
            SELECT Node_Instance_ID, Workflow_Instance_ID, Node_Type_Value, Config_Node_ID,
                   Node_Instance_Number, Node_Instance_Status_Value,
                   Node_Instance_System_Status_Value, Node_Instance_Business_Status_Value,
                   Start_Date_Time, Completed_Date_Time, Failure_Reason
            FROM Workflow.WF_EXEC_Node_Instance
            WHERE Node_Instance_Number = @NodeInstanceNumber
              AND Is_Active_Inactive_Deleted_Flag = 'A';";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NodeInstanceNumber", nodeInstanceNumber);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return null;

            return new NodeInstance
            {
                Node_Instance_ID = reader.GetInt64(0),
                Workflow_Instance_ID = reader.GetInt64(1),
                Node_Type_Value = reader.GetString(2),
                Config_Node_ID = reader.GetInt64(3),
                Node_Instance_Number = reader.GetString(4),
                Node_Instance_Status_Value = reader.GetString(5),
                Node_Instance_System_Status_Value = reader.GetString(6),
                Node_Instance_Business_Status_Value = reader.GetString(7),
                Start_Date_Time = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                Completed_Date_Time = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                Failure_Reason = reader.IsDBNull(10) ? null : reader.GetString(10)
            };
        }

        public async Task UpdateStatusAsync(
            long nodeInstanceId, string status, string systemStatus, string businessStatus,
            DateTime? completedDateTime, string? failureReason, CancellationToken ct)
        {
            const string sql = @"
            UPDATE Workflow.WF_EXEC_Node_Instance
            SET Node_Instance_Status_Value = @Status,
                Node_Instance_System_Status_Value = @SystemStatus,
                Node_Instance_Business_Status_Value = @BusinessStatus,
                Completed_Date_Time = @CompletedDateTime,
                Failure_Reason = @FailureReason,
                Updated_Date_Time = SYSUTCDATETIME()
            WHERE Node_Instance_ID = @NodeInstanceId;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@NodeInstanceId", nodeInstanceId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@SystemStatus", systemStatus);
            cmd.Parameters.AddWithValue("@BusinessStatus", businessStatus);
            cmd.Parameters.AddWithValue("@CompletedDateTime", completedDateTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@FailureReason", failureReason ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
