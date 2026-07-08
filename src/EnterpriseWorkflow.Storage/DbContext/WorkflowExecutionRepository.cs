using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Storage.DbContext
{
    public class WorkflowExecutionRepository : IWorkflowExecutionRepository
    {
        private readonly string _connectionString;

        public WorkflowExecutionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("EnterpriseDb");
        }
        public async Task<long> InsertAsync(WorkflowExecution e, CancellationToken ct)
        {
            const string sql = @"
            INSERT INTO Workflow.WF_EXEC_Workflow_Execution
                (Workflow_Instance_ID, Workflow_Execution_Number, Execution_Sequence_Number,
                 Execution_Type_Code, Workflow_Execution_Status_Value, Workflow_Execution_System_Status_Value,
                 Workflow_Execution_Business_Status_Value, Created_By, Updated_By, Status_Code)
            OUTPUT INSERTED.Workflow_Execution_ID
            VALUES
                (@WorkflowInstanceId, @ExecutionNumber, @SequenceNumber, @ExecutionTypeCode,
                 @StatusValue, @SystemStatus, @BusinessStatus, @CreatedBy, @UpdatedBy, @StatusCode);";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@WorkflowInstanceId", e.Workflow_Instance_ID);
            cmd.Parameters.AddWithValue("@ExecutionNumber", e.Workflow_Execution_Number);
            cmd.Parameters.AddWithValue("@SequenceNumber", e.Execution_Sequence_Number);
            cmd.Parameters.AddWithValue("@ExecutionTypeCode", e.Execution_Type_Code);
            cmd.Parameters.AddWithValue("@StatusValue", e.Workflow_Execution_Status_Value);
            cmd.Parameters.AddWithValue("@SystemStatus", e.Workflow_Execution_System_Status_Value);
            cmd.Parameters.AddWithValue("@BusinessStatus", e.Workflow_Execution_Business_Status_Value);
            cmd.Parameters.AddWithValue("@CreatedBy", e.Created_By);
            cmd.Parameters.AddWithValue("@UpdatedBy", e.Updated_By);
            cmd.Parameters.AddWithValue("@StatusCode", e.Status_Code);

            return (long)(await cmd.ExecuteScalarAsync(ct))!;
        }

        public async Task UpdateStatusAsync(
            long workflowExecutionId, string status, string systemStatus, string businessStatus,
            DateTime? endDateTime, string? failureReason, CancellationToken ct)
        {
            const string sql = @"
            UPDATE Workflow.WF_EXEC_Workflow_Execution
            SET Workflow_Execution_Status_Value = @Status,
                Workflow_Execution_System_Status_Value = @SystemStatus,
                Workflow_Execution_Business_Status_Value = @BusinessStatus,
                End_Date_Time = @EndDateTime,
                Failure_Reason = @FailureReason,
                Updated_Date_Time = SYSUTCDATETIME()
            WHERE Workflow_Execution_ID = @WorkflowExecutionId;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@WorkflowExecutionId", workflowExecutionId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@SystemStatus", systemStatus);
            cmd.Parameters.AddWithValue("@BusinessStatus", businessStatus);
            cmd.Parameters.AddWithValue("@EndDateTime", endDateTime ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@FailureReason", failureReason ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync(ct);
        }

    }

}