// EnterpriseWorkflow.Storage/Implementations/Repositories/SqlRuleRepository.cs
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Entities.RuleEngine;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnterpriseWorkflow.Storage.Implementations.Repositories;

public class SqlRuleRepository : IRuleRepository
{
    private readonly string _connectionString;
    private readonly ILogger<SqlRuleRepository> _logger;

    public SqlRuleRepository(IConfiguration configuration, ILogger<SqlRuleRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("EnterpriseDb")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:EnterpriseDb");
        _logger = logger;
    }

    public async Task<RuleSetDefinition?> GetByWorkflowNameAsync(string workflowName, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT Rule_Set_ID, Rule_Set_Name, Rule_Set_Type_Code, Evaluation_Mode_Code, Workflow_Name
            FROM Workflow.WF_CONFIG_Rule_Set
            WHERE Workflow_Name = @Name
              AND Is_Active_Inactive_Deleted_Flag = 'A';";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Name", workflowName);

        long ruleSetId;
        RuleSetDefinition ruleSet;

        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            if (!await reader.ReadAsync(ct))
            {
                _logger.LogWarning("[SqlRuleRepository] No rule set found for workflowName='{Name}'", workflowName);
                return null;
            }

            ruleSetId = reader.GetInt64(0);
            ruleSet = new RuleSetDefinition
            {
                RuleSetId = (int)ruleSetId,
                RuleSetName = reader.GetString(1),
                RuleSetType = reader.IsDBNull(2) ? "Routing" : reader.GetString(2),
                Strategy = MapStrategy(reader.IsDBNull(3) ? null : reader.GetString(3)),
                WorkflowName = reader.IsDBNull(4) ? workflowName : reader.GetString(4)
            };
        }

        ruleSet.Rules = await LoadRulesAsync(conn, ruleSetId, ct);
        return ruleSet;
    }

    public async Task<RuleSetDefinition?> GetByIdAsync(int ruleSetId, CancellationToken ct = default)
    {
        const string sql = @"
            SELECT Rule_Set_ID, Rule_Set_Name, Rule_Set_Type_Code, Evaluation_Mode_Code, Workflow_Name
            FROM Workflow.WF_CONFIG_Rule_Set
            WHERE Rule_Set_ID = @Id
              AND Is_Active_Inactive_Deleted_Flag = 'A';";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", ruleSetId);

        RuleSetDefinition ruleSet;
        await using (var reader = await cmd.ExecuteReaderAsync(ct))
        {
            if (!await reader.ReadAsync(ct)) return null;

            ruleSet = new RuleSetDefinition
            {
                RuleSetId = ruleSetId,
                RuleSetName = reader.GetString(1),
                RuleSetType = reader.IsDBNull(2) ? "Routing" : reader.GetString(2),
                Strategy = MapStrategy(reader.IsDBNull(3) ? null : reader.GetString(3)),
                WorkflowName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
            };
        }

        ruleSet.Rules = await LoadRulesAsync(conn, ruleSetId, ct);
        return ruleSet;
    }

    public async Task<IReadOnlyList<RuleSetDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        const string sql = @"
            SELECT Rule_Set_ID
            FROM Workflow.WF_CONFIG_Rule_Set
            WHERE Is_Active_Inactive_Deleted_Flag = 'A';";

        var ids = new List<long>();
        await using (var conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                ids.Add(reader.GetInt64(0));
        }

        var result = new List<RuleSetDefinition>();
        foreach (var id in ids)
        {
            var rs = await GetByIdAsync((int)id, ct);
            if (rs is not null) result.Add(rs);
        }
        return result;
    }

    private static async Task<List<RuleDefinition>> LoadRulesAsync(
        SqlConnection conn, long ruleSetId, CancellationToken ct)
    {
        // Engine_Activity_Reference IS NULL excludes the workflow-node config row
        // (e.g. 'EvaluateRules') and keeps only actual evaluatable business rules.
        const string sql = @"
            SELECT Rule_ID, Rule_Set_ID, Rule_Name, Priority_Number,
                   Evaluation_Type_Code, Evaluator_Reference, Success_Outcome_Code,
                   (CASE WHEN Is_Active_Inactive_Deleted_Flag = 'A' THEN 1 ELSE 0 END)
            FROM Workflow.WF_CONFIG_Rule
            WHERE Rule_Set_ID = @RuleSetId
              AND Engine_Activity_Reference IS NULL
              AND Is_Active_Inactive_Deleted_Flag = 'A'
            ORDER BY Priority_Number;";

        var rules = new List<RuleDefinition>();
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@RuleSetId", ruleSetId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            rules.Add(new RuleDefinition
            {
                RuleId = (int)reader.GetInt64(0),
                RuleSetId = (int)reader.GetInt64(1),
                RuleName = reader.GetString(2),
                Priority = reader.IsDBNull(3) ? 1 : reader.GetInt32(3),
                EvaluationType = reader.IsDBNull(4) ? "Expression" : reader.GetString(4),
                EvaluatorReference = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                SuccessEvent = reader.IsDBNull(6) ? null : reader.GetString(6),
                IsActive = reader.GetInt32(7) == 1
            });
        }
        return rules;
    }

    private static string MapStrategy(string? evaluationModeCode) => evaluationModeCode switch
    {
        "EVALUATE_ALL" => "AllMatches",
        "FIRST_MATCH" => "FirstMatchWins",
        "HIGHEST_PRIORITY" => "HighestPriorityWins",
        _ => "AllMatches"
    };
}