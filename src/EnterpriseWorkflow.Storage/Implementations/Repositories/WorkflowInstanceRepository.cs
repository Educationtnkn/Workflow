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

public class WorkflowInstanceRepository : IWorkflowInstanceRepository
{

    private readonly string _connectionString;

    public WorkflowInstanceRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("EnterpriseDb");
    }

    public async Task<long> InsertAsync(WorkflowInstance e, CancellationToken ct)
    {
        const string sql = @"
            INSERT INTO Workflow.WF_EXEC_Workflow_Instance
                (Workflow_Version_ID, Org_ID, Domain_ID, Workflow_Instance_Number,
                 Business_Reference_Type_Code, Business_Reference_ID,
                 Workflow_Instance_Status_Value, Workflow_Instance_System_Status_Value,
                 Workflow_Instance_Business_Status_Value, Start_Date_Time,
                 Created_By, Updated_By, Status_Code)
            OUTPUT INSERTED.Workflow_Instance_ID
            VALUES
                (@WorkflowVersionId, @OrgId, @DomainId, @InstanceNumber,
                 @BusinessRefType, @BusinessRefId,
                 @StatusValue, @SystemStatus, @BusinessStatus, @StartDateTime,
                 @CreatedBy, @UpdatedBy, @StatusCode);";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@WorkflowVersionId", e.Workflow_Version_ID);
        cmd.Parameters.AddWithValue("@OrgId", e.Org_ID);
        cmd.Parameters.AddWithValue("@DomainId", e.Domain_ID);
        cmd.Parameters.AddWithValue("@InstanceNumber", e.Workflow_Instance_Number);
        cmd.Parameters.AddWithValue("@BusinessRefType", e.Business_Reference_Type_Code);
        cmd.Parameters.AddWithValue("@BusinessRefId", e.Business_Reference_ID);
        cmd.Parameters.AddWithValue("@StatusValue", e.Workflow_Instance_Status_Value);
        cmd.Parameters.AddWithValue("@SystemStatus", e.Workflow_Instance_System_Status_Value);
        cmd.Parameters.AddWithValue("@BusinessStatus", e.Workflow_Instance_Business_Status_Value);
        cmd.Parameters.AddWithValue("@StartDateTime", e.Start_Date_Time ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@CreatedBy", e.Created_By);
        cmd.Parameters.AddWithValue("@UpdatedBy", e.Updated_By);
        cmd.Parameters.AddWithValue("@StatusCode", e.Status_Code);

        return (long)(await cmd.ExecuteScalarAsync(ct))!;
    }

    public async Task<WorkflowInstance?> GetAsync(long workflowInstanceId, CancellationToken ct)
        => await GetByPredicateAsync("Workflow_Instance_ID = @Key", workflowInstanceId, ct);

    public async Task<WorkflowInstance?> GetByInstanceNumberAsync(string workflowInstanceNumber, CancellationToken ct)
        => await GetByPredicateAsync("Workflow_Instance_Number = @Key", workflowInstanceNumber, ct);

    public async Task<WorkflowInstance?> GetByInstanceNumberUsingEngineInstanceNumberAsync(string EngineInstanceNumber, CancellationToken ct)
    => await GetByPredicateAsync("Engine_Instance_Reference = @Key", EngineInstanceNumber, ct);

    private async Task<WorkflowInstance?> GetByPredicateAsync(string predicate, object key, CancellationToken ct)
    {
        var sql = $@"
            SELECT Workflow_Instance_ID, Parent_Workflow_Instance_ID, Workflow_Version_ID, Org_ID, Domain_ID,
                   Workflow_Instance_Number, Business_Reference_Type_Code, Business_Reference_ID,
                   Business_Reference_Number, Workflow_Instance_Status_Value,
                   Workflow_Instance_System_Status_Value, Workflow_Instance_Business_Status_Value,
                   Start_Date_Time, Completed_Date_Time, Cancelled_Date_Time, Failure_Reason
            FROM Workflow.WF_EXEC_Workflow_Instance
            WHERE {predicate};";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Key", key);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        return new WorkflowInstance
        {
            Workflow_Instance_ID = reader.GetInt64(0),
            Parent_Workflow_Instance_ID = reader.IsDBNull(1) ? null : reader.GetInt64(1),
            Workflow_Version_ID = reader.GetInt64(2),
            Org_ID = reader.GetInt32(3),
            Domain_ID = reader.GetInt64(4),
            Workflow_Instance_Number = reader.GetString(5),
            Business_Reference_Type_Code = reader.GetString(6),
            Business_Reference_ID = reader.GetInt64(7),
            Business_Reference_Number = reader.IsDBNull(8) ? null : reader.GetString(8),
            Workflow_Instance_Status_Value = reader.GetString(9),
            Workflow_Instance_System_Status_Value = reader.GetString(10),
            Workflow_Instance_Business_Status_Value = reader.GetString(11),
            Start_Date_Time = reader.IsDBNull(12) ? null : reader.GetDateTime(12),
            Completed_Date_Time = reader.IsDBNull(13) ? null : reader.GetDateTime(13),
            Cancelled_Date_Time = reader.IsDBNull(14) ? null : reader.GetDateTime(14),
            Failure_Reason = reader.IsDBNull(15) ? null : reader.GetString(15)
        };
    }

    public async Task<CurrentStatusDto> GetCurrentStatusAsync(long workflowInstanceId, CancellationToken ct)
    {
        const string sql = @"
            SELECT Workflow_Instance_Status_Value, Workflow_Instance_System_Status_Value,
                   Workflow_Instance_Business_Status_Value
            FROM Workflow.WF_EXEC_Workflow_Instance
            WHERE Workflow_Instance_ID = @Id;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", workflowInstanceId);

        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
            return new CurrentStatusDto("Unknown", "UNKNOWN", "Unknown");

        return new CurrentStatusDto(reader.GetString(0), reader.GetString(1), reader.GetString(2));
    }

    public async Task UpdateStatusAsync(
        long workflowInstanceId, string status, string systemStatus, string businessStatus,
        CancellationToken ct, string? failureReason = null)
    {
        const string sql = @"
            UPDATE Workflow.WF_EXEC_Workflow_Instance
            SET Workflow_Instance_Status_Value = @Status,
                Workflow_Instance_System_Status_Value = @SystemStatus,
                Workflow_Instance_Business_Status_Value = @BusinessStatus,
                Completed_Date_Time = CASE WHEN @Status = 'Completed' THEN SYSUTCDATETIME() ELSE Completed_Date_Time END,
                Cancelled_Date_Time = CASE WHEN @Status = 'Cancelled' THEN SYSUTCDATETIME() ELSE Cancelled_Date_Time END,
                Failure_Reason = @FailureReason,
                Last_Activity_Date_Time = SYSUTCDATETIME(),
                Updated_Date_Time = SYSUTCDATETIME()
                 
            WHERE Workflow_Instance_ID = @Id;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", workflowInstanceId);
        cmd.Parameters.AddWithValue("@Status", status);
        cmd.Parameters.AddWithValue("@SystemStatus", systemStatus);
        cmd.Parameters.AddWithValue("@BusinessStatus", businessStatus);
        cmd.Parameters.AddWithValue("@FailureReason", failureReason ?? (object)DBNull.Value);

        await cmd.ExecuteNonQueryAsync(ct);
    }

    Task<WorkflowInstance?> IWorkflowInstanceRepository.GetByPredicateAsync(string predicate, object key, CancellationToken ct)
    {
        return GetByPredicateAsync(predicate, key, ct);
    }


    public async Task UpdateEngineWorkflowInstanceAsync(
     string workflowInstanceNumber, string enterpriseInstanceId,
        CancellationToken ct, string? failureReason = null)
    {
        const string sql = @"
            UPDATE Workflow.WF_EXEC_Workflow_Instance
            SET Engine_Instance_Reference = @Engine_Instance_Reference
                 
            WHERE Workflow_Instance_Number = @enterpriseInstanceId;";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Engine_Instance_Reference", workflowInstanceNumber);

        cmd.Parameters.AddWithValue("@enterpriseInstanceId", enterpriseInstanceId);
        await cmd.ExecuteNonQueryAsync(ct);
    }
}
