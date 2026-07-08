using EnterpriseWorkflow.Application.Model;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static EnterpriseWorkflow.Application.Ports.Outbound.Implemenation.WorkFlowEngine;

namespace EnterpriseWorkflow.Application.Services
{
    public class WorkflowPublishService : IWorkflowPublishService
    {
        private readonly string _connectionString;
        private readonly IBuildPublishElsaDefinition _elsaPublishAdapter;

        public WorkflowPublishService(
            IConfiguration configuration,
            IBuildPublishElsaDefinition elsaPublishAdapter)
        {
            _connectionString = configuration.GetConnectionString("EnterpriseDb")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:EnterpriseDb");
            _elsaPublishAdapter = elsaPublishAdapter;
        }

        public async Task<bool> PublishAsync(long workflowVersionId, CancellationToken ct)
        {
            // All private structs gone — use Application Model DTOs directly
            var (workflowId, workflowCode, workflowDesc) =
                await LoadWorkflowByVersionAsync(workflowVersionId, ct);

            var (orgId, domainId, versionNumber) =
                await LoadVersionMetaAsync(workflowVersionId, ct);

            var nodes = await LoadAllNodesAsync(workflowVersionId, ct);
            var transitions = await LoadTransitionsAsync(workflowVersionId, ct);

            var request = new WorkflowPublishRequest(
                WorkflowCode: workflowCode,
                WorkflowDescription: workflowDesc,
                OrgId: orgId,
                DomainId: domainId,
                VersionNumber: versionNumber,
                Nodes: nodes,
                Transitions: transitions
            );

            var result = await _elsaPublishAdapter.BuildElsaDefinition(request, ct);

            if (!result.Success)
                throw new InvalidOperationException(
                    $"Elsa publish failed: {result.ErrorMessage}");

            await MarkPublishedAsync(workflowVersionId, ct);
            return true;
        }

        // ── ADO.NET — returns Application Model DTOs, no private records ─────────

        private async Task<(long WorkflowId, string WorkflowCode, string? Description)>
            LoadWorkflowByVersionAsync(long versionId, CancellationToken ct)
        {
            const string sql = @"
            SELECT w.Workflow_ID, w.Workflow_Code, w.Workflow_Description
            FROM Workflow.WF_CONFIG_Workflows w
            INNER JOIN Workflow.WF_CONFIG_Workflow_Version v
                ON v.Workflow_ID = w.Workflow_ID
            WHERE v.Workflow_Version_ID = @Id;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", versionId);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            await r.ReadAsync(ct);
            return (r.GetInt64(0), r.GetString(1),
                    r.IsDBNull(2) ? null : r.GetString(2));
        }

        private async Task<(int OrgId, long DomainId, string VersionNumber)>
            LoadVersionMetaAsync(long versionId, CancellationToken ct)
        {
            const string sql = @"
            SELECT Org_ID, Domain_ID, Version_Number
            FROM Workflow.WF_CONFIG_Workflow_Version
            WHERE Workflow_Version_ID = @Id;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", versionId);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            await r.ReadAsync(ct);
            return (r.GetInt32(0), r.GetInt64(1), r.GetString(2));
        }

        private async Task<List<WorkflowNodeDto>> LoadAllNodesAsync(
            long versionId, CancellationToken ct)
        {
            const string sql = @"
            SELECT 'ACTION', a.Action_Definition_ID,
                   a.Engine_Activity_Reference, a.Action_CONFIG_Json,
                   ROW_NUMBER() OVER (ORDER BY a.Action_Definition_ID),Type
            FROM Workflow.WF_CONFIG_Actions a
            INNER JOIN Workflow.WF_CONFIG_Workflow_Version v
                ON v.Org_ID = a.Org_ID AND v.Domain_ID = a.Domain_ID
            WHERE v.Workflow_Version_ID = @VersionId
              AND a.Is_Active_Inactive_Deleted_Flag = 'A'
              AND a.Engine_Activity_Reference IS NOT NULL

            UNION ALL

            SELECT 'STEP', s.Step_ID,
                   s.Engine_Activity_Reference, s.Step_CONFIG_Json,
                   ROW_NUMBER() OVER (ORDER BY s.Step_ID),Type
            FROM Workflow.WF_CONFIG_Steps s
            INNER JOIN Workflow.WF_CONFIG_Workflow_Version v
                ON v.Org_ID = s.Org_ID AND v.Domain_ID = s.Domain_ID
            WHERE v.Workflow_Version_ID = @VersionId
              AND s.Is_Active_Inactive_Deleted_Flag = 'A'
              AND s.Engine_Activity_Reference IS NOT NULL

            UNION ALL

            SELECT 'TASK', t.Task_ID,
                   t.Engine_Activity_Reference, t.Task_CONFIG_Json,
                   ROW_NUMBER() OVER (ORDER BY t.Task_ID),Type
            FROM Workflow.WF_CONFIG_Tasks t
            INNER JOIN Workflow.WF_CONFIG_Workflow_Version v
                ON v.Org_ID = t.Org_ID AND v.Domain_ID = t.Domain_ID
            WHERE v.Workflow_Version_ID = @VersionId
              AND t.Is_Active_Inactive_Deleted_Flag = 'A'
              AND t.Engine_Activity_Reference IS NOT NULL

            UNION ALL

            SELECT 'RULE', r.Rule_ID,
                   r.Engine_Activity_Reference, r.Rule_CONFIG_Json,
                   ROW_NUMBER() OVER (ORDER BY r.Rule_ID),Type
            FROM Workflow.WF_CONFIG_Rule r
            INNER JOIN Workflow.WF_CONFIG_Workflow_Version v
                ON v.Org_ID = r.Org_ID AND v.Domain_ID = r.Domain_ID
            WHERE v.Workflow_Version_ID = @VersionId
              AND r.Is_Active_Inactive_Deleted_Flag = 'A'
              AND r.Engine_Activity_Reference IS NOT NULL;";

            var result = new List<WorkflowNodeDto>();
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VersionId", versionId);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                result.Add(new WorkflowNodeDto(
                    NodeTableType: r.GetString(0),
                    NodeId: r.GetInt64(1),
                    EngineActivityReference: r.GetString(2),
                    ConfigJson: r.IsDBNull(3) ? null : r.GetString(3),
                    SequenceNumber: r.GetInt64(4)));
                    //Type: r.IsDBNull(5) ? null : r.GetString(5)));
            return result;
        }

        private async Task<List<WorkflowTransitionDto>> LoadTransitionsAsync(
            long versionId, CancellationToken ct)
        {
            const string sql = @"
            SELECT From_Node_Type_Code, From_Node_ID,
                   To_Node_Type_Code,   To_Node_ID,
                   Transition_Path_Type_Code, Sequence_Number
            FROM Workflow.WF_CONFIG_Workflow_Transitions
            WHERE Workflow_Version_ID = @VersionId
              AND Is_Active_Inactive_Deleted_Flag = 'A'
            ORDER BY Sequence_Number;";

            var result = new List<WorkflowTransitionDto>();
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@VersionId", versionId);
            await using var r = await cmd.ExecuteReaderAsync(ct);
            while (await r.ReadAsync(ct))
                result.Add(new WorkflowTransitionDto(
                    FromNodeType: r.GetString(0),
                    FromNodeId: r.GetInt64(1),
                    ToNodeType: r.GetString(2),
                    ToNodeId: r.GetInt64(3),
                    TransitionPathType: r.IsDBNull(4) ? "DONE" : r.GetString(4),
                    SequenceNumber: r.GetInt32(5)));
            return result;
        }

        private async Task MarkPublishedAsync(long versionId, CancellationToken ct)
        {
            const string sql = @"
            UPDATE Workflow.WF_CONFIG_Workflow_Version
            SET Is_Published_Flag   = 1,
                Published_Date_Time = SYSUTCDATETIME(),
                Updated_Date_Time   = SYSUTCDATETIME()
            WHERE Workflow_Version_ID = @Id;";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", versionId);
            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}