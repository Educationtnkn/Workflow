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
    public class ConfigNodeLookupRepository : IConfigNodeLookupRepository
    {
        private readonly string _connectionString;

        public ConfigNodeLookupRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("EnterpriseDb");
        }

        public async Task<ConfigNodeResult?> GetByEngineActivityReferenceAsync(
            string engineActivityReference, CancellationToken ct)
        {
            // Search all node tables — first match wins (Engine_Activity_Reference is unique per workflow)
            const string sql = @"
            SELECT TOP 1 NodeType, NodeId FROM (
                SELECT 'ACTION' AS NodeType, Action_Definition_ID AS NodeId
                FROM Workflow.WF_CONFIG_Actions
                WHERE Engine_Activity_Reference = @Ref AND Is_Active_Inactive_Deleted_Flag = 'A'

                UNION ALL

                SELECT 'STEP', Step_ID
                FROM Workflow.WF_CONFIG_Steps
                WHERE Engine_Activity_Reference = @Ref AND Is_Active_Inactive_Deleted_Flag = 'A'

                UNION ALL

                SELECT 'TASK', Task_ID
                FROM Workflow.WF_CONFIG_Tasks
                WHERE Engine_Activity_Reference = @Ref AND Is_Active_Inactive_Deleted_Flag = 'A'

                UNION ALL

                SELECT 'STAGE', Stage_ID
                FROM Workflow.WF_CONFIG_Stages
                WHERE Engine_Activity_Reference = @Ref AND Is_Active_Inactive_Deleted_Flag = 'A'

                UNION ALL

                SELECT 'RULE', Rule_ID
                FROM Workflow.WF_CONFIG_Rule
                WHERE Engine_Activity_Reference = @Ref AND Is_Active_Inactive_Deleted_Flag = 'A'
            ) x;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Ref", engineActivityReference);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            if (!await reader.ReadAsync(ct)) return null;

            return new ConfigNodeResult(reader.GetInt64(1), reader.GetString(0));
        }
    }
}
