using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.ValueObjects;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Services
{
    public class WorkflowEventTriggerService : IWorkflowEventTriggerService
    {
        private readonly string _connectionString;
        private readonly IWaitStateRepository _waitStateRepo;
        private readonly IWorkflowInstanceRepository _instanceRepo;
        private readonly IWorkflowEngineAdapter _engineAdapter;

        public WorkflowEventTriggerService(
            IConfiguration configuration,
            IWaitStateRepository waitStateRepo,
            IWorkflowInstanceRepository instanceRepo,
            IWorkflowEngineAdapter engineAdapter)
        {
            _connectionString = configuration.GetConnectionString("EnterpriseDb")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:EnterpriseDb");
            _waitStateRepo = waitStateRepo;
            _instanceRepo = instanceRepo;
            _engineAdapter = engineAdapter;
        }

        public async Task<TriggerEventResponse> TriggerAsync(
            TriggerEventRequest request, ExecutionModel ctx, CancellationToken ct)
        {
            // 1. Resolve event config (elsaEventName lives in Event_CONFIG_Json)
            var elsaEventName = await ResolveElsaEventNameAsync(request.EventCode, ct);
            if (elsaEventName is null)
                throw new KeyNotFoundException($"No active event configured for code '{request.EventCode}'.");

            // 2. Find the active wait state by enterprise correlation key
            var waitState = await _waitStateRepo.GetActiveByCorrelationKeyAsync(
                request.WorkflowInstanceNumber, ct);
            if (waitState is null)
                throw new KeyNotFoundException(
                    $"No active wait state found for workflow instance '{request.WorkflowInstanceNumber}'. " +
                    "The workflow may not be suspended, or may already have been resumed.");

            // 3. Resolve enterprise instance -> Elsa instance id
            var instance = await _instanceRepo.GetAsync(waitState.Workflow_Instance_ID, ct);
            if (instance is null)
                throw new KeyNotFoundException($"Workflow instance {waitState.Workflow_Instance_ID} not found.");

            var elsaInstanceId = await GetElsaInstanceIdAsync(waitState.Workflow_Instance_ID, ct);
            if (string.IsNullOrWhiteSpace(elsaInstanceId))
                throw new InvalidOperationException(
                    $"Enterprise instance {waitState.Workflow_Instance_ID} has no linked Elsa instance id. " +
                    "Was it started before the Elsa_Workflow_Instance_Id column was populated?");

            // 4. Resume Elsa
            var resumeResult = await _engineAdapter.ResumeWorkflowAsync(
                new WorkflowResumeRequest
                {
                    workflowExecutionId = elsaInstanceId,
                    Task = elsaEventName,
                    Action = request.Action,
                    Payload = request.Payload
                },
                ctx,
                ct);

            // 5. Mark enterprise wait state resumed
            await _waitStateRepo.MarkResumedAsync(waitState.Wait_State_ID, ct);

            return new TriggerEventResponse
            {
                WorkflowInstanceNumber = request.WorkflowInstanceNumber,
                EventCode = request.EventCode,
                Status = resumeResult.Status,
                SubStatus = resumeResult.SubStatus
            };
        }

        private async Task<string?> ResolveElsaEventNameAsync(string eventCode, CancellationToken ct)
        {
            const string sql = @"
            SELECT Event_CONFIG_Json
            FROM Workflow.WF_CONFIG_Events
            WHERE Event_Code = @Code AND Is_Active_Inactive_Deleted_Flag = 'A';";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Code", eventCode);

            var configJson = (string?)await cmd.ExecuteScalarAsync(ct);
            if (string.IsNullOrWhiteSpace(configJson)) return null;

            using var doc = JsonDocument.Parse(configJson);
            return doc.RootElement.TryGetProperty("elsaEventName", out var v) ? v.GetString() : null;
        }

        private async Task<string?> GetElsaInstanceIdAsync(long enterpriseInstanceId, CancellationToken ct)
        {
            const string sql = @"
            SELECT Engine_Instance_Reference
            FROM Workflow.WF_EXEC_Workflow_Instance
            WHERE Workflow_Instance_ID = @Id;";
                                                                  
            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync(ct);
            await using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", enterpriseInstanceId);
            return (string?)await cmd.ExecuteScalarAsync(ct);
        }
    }
}
