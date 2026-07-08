using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Utilities.Observability;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EnterpriseWorkflow.Storage.Implementations.Repositories
{
    public sealed class WorkflowActivityLogRepository

    {
        private readonly string _connectionString;
        private readonly ILoggingService _logger;

        private const string Sp = "dbo.usp_InsertWorkflowActionLog";

        public WorkflowActivityLogRepository(
            IConfiguration configuration,
            ILoggingService logger)
        {
            _connectionString = configuration.GetConnectionString("EnterpriseDb")
                ?? throw new InvalidOperationException(
                    "Connection string 'EnterpriseDb' not found in configuration.");
            _logger = logger;
        }

        /// <summary>
        /// Calls usp_InsertWorkflowActivityLog and returns the new IDENTITY Id.
        /// Never throws — swallows and logs failures so the workflow is never disrupted.
        /// </summary>
        public async Task<long?> InsertAsync(
            WorkflowActionLog entry,
            CancellationToken ct = default)
        {
            try
            {
                await using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync(ct);

                await using var cmd = new SqlCommand(Sp, conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 30
                };
                _logger.LogInformation("InsertDb", $"[WorkflowActivityLog] Insert started for instance id {entry.WorkflowInstanceId}");

                // ── input params ────────────────────────────────────────────
                cmd.Parameters.Add("@WorkflowInstanceId", SqlDbType.NVarChar, 100).Value = entry.WorkflowInstanceId;
                cmd.Parameters.Add("@NodeType", SqlDbType.NVarChar, 200).Value = entry.NodeType;
                cmd.Parameters.Add("@Id", SqlDbType.NVarChar, 200).Value = entry.ActionId;
                cmd.Parameters.Add("@ElsaNodeType", SqlDbType.NVarChar, 200).Value = entry.ActionType;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 100).Value = entry.Status;
                cmd.Parameters.Add("@HasBookmarks", SqlDbType.Bit).Value = entry.HasBookmarks;
                cmd.Parameters.Add("@CreatedAt", SqlDbType.DateTime2).Value = entry.LoggedAt;

                cmd.Parameters.Add("@BusinessMessage", SqlDbType.NVarChar, 1000).Value = (object?)entry.BusinessMessage ?? DBNull.Value;
                cmd.Parameters.Add("@StartedAt", SqlDbType.DateTimeOffset).Value = (object?)entry.StartedAt ?? DBNull.Value;
                cmd.Parameters.Add("@CompletedAt", SqlDbType.DateTimeOffset).Value = (object?)entry.CompletedAt ?? DBNull.Value;
                cmd.Parameters.Add("@TenantId", SqlDbType.NVarChar, 100).Value = (object?)entry.TenantId ?? DBNull.Value;
                cmd.Parameters.Add("@Error", SqlDbType.NVarChar, -1).Value = (object?)entry.SerializedException ?? DBNull.Value; // -1 = MAX

                // ── output param ────────────────────────────────────────────
                var outId = new SqlParameter("@NewId", SqlDbType.BigInt)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outId);

                await cmd.ExecuteNonQueryAsync(ct);

                _logger.LogInformation("InsertDb", $"[WorkflowActivityLog] Insert instance id {entry.WorkflowInstanceId} successfully");

                return outId.Value is DBNull ? null : Convert.ToInt64(outId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError("InsertDb",
                    $"[WorkflowActivityLog] Insert failed — InstanceId={entry.WorkflowInstanceId} ActionId={entry.ActionId} Status={entry.Status}");
                return null;
            }
        }
    }
}
