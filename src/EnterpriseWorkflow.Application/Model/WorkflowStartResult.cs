using EnterpriseWorkflow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Model
{
        public sealed class WorkflowStartResult
        {
            /// <summary>
            /// Enterprise execution ID (your system's ID)
            /// </summary>
            public string ExecutionId { get; set; } = default!;

            /// <summary>
            /// Engine-specific execution ID (e.g., Elsa's WorkflowInstance ID)
            /// </summary>
            public string EngineExecutionId { get; set; } = default!;

            /// <summary>
            /// Status after start (Running, Suspended, etc.)
            /// </summary>
            public WorkflowExecutionStatus Status { get; set; } = WorkflowExecutionStatus.Running;
            public WorkflowExecutionSubStatus SubStatus { get; set; } 


            /// <summary>
            /// Timestamp when workflow started
            /// </summary>
            public DateTimeOffset? StartedAt { get; set; } = DateTimeOffset.UtcNow;

            public static WorkflowStartResult Create(string executionId, string engineExecutionId)
            {
                return new WorkflowStartResult
                {
                    ExecutionId = executionId,
                    EngineExecutionId = engineExecutionId,
                    Status = WorkflowExecutionStatus.Running,
                    StartedAt = DateTimeOffset.UtcNow
                };
            }
            public List<BookmarkResponseDto>? Bookmark { get; set; }
        }
    public class BookmarkResponseDto
    {
        public string BookmarkId { get; set; } = string.Empty;  // ← Ids
        public string ActionName { get; set; } = string.Empty;  // ← Name
        public string ActionId { get; set; } = string.Empty;  // ← ActivityId
    }

    public sealed class WorkflowExecutionState
        {
            public string ExecutionId { get; set; } = default!;
            public WorkflowExecutionStatus Status { get; set; }
            public WorkflowExecutionSubStatus SubStatus { get; set; }
            public DateTimeOffset StartedAt { get; set; }
            public DateTimeOffset? CompletedAt { get; set; }
            public Dictionary<string, object> Variables { get; set; } = new();
            public string? CurrentActivityId { get; set; }
            public string? CurrentActivityType { get; set; }
            public List<ActivityState> ActivityHistory { get; set; } = new();
            public List<ExecutionError> Errors { get; set; } = new();
        }

        public sealed class WorkflowExecutionSummary
        {
            public string ExecutionId { get; set; } = default!;
            public string EngineExecutionId { get; set; } = default!;
            public WorkflowExecutionStatus Status { get; set; }
            public DateTimeOffset StartedAt { get; set; }
            public DateTimeOffset? CompletedAt { get; set; }
            public string? CorrelationId { get; set; }
            public string? TriggeredBy { get; set; }
        }

        public sealed class ActivityState
        {
            public string ActivityId { get; set; } = default!;
            public string ActivityType { get; set; } = default!;
            public string? ActivityName { get; set; }
            public DateTimeOffset StartedAt { get; set; }
            public DateTimeOffset? CompletedAt { get; set; }
            public ActivityStatus Status { get; set; }
            public Dictionary<string, object>? Outputs { get; set; }
        }

        public sealed class ExecutionError
        {
            public string ErrorId { get; set; } = default!;
            public string Message { get; set; } = default!;
            public string? StackTrace { get; set; }
            public DateTimeOffset OccurredAt { get; set; }
            public int RetryAttempt { get; set; }
            public string? ActivityId { get; set; }
        }

        public sealed class PagedResult<T>
        {
            public List<T> Items { get; set; } = new();
            public int TotalCount { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public bool HasNextPage => Page * PageSize < TotalCount;
            public bool HasPreviousPage => Page > 1;
        }

        public sealed class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<ValidationError> Errors { get; set; } = new();

            public static ValidationResult Success() => new() { IsValid = true };
            public static ValidationResult Fail(string error) =>
                new() { IsValid = false, Errors = { new ValidationError(error) } };
        }

        public sealed class ValidationError
        {
            public string ErrorCode { get; set; } = default!;
            public string Message { get; set; } = default!;
            public string? PropertyPath { get; set; }

            public ValidationError(string message) => Message = message;
        }

        public sealed class EngineHealthStatus
        {
            public bool IsHealthy { get; set; }
            public string EngineType { get; set; } = default!;
            public string? Version { get; set; }
            public DateTimeOffset LastHeartbeat { get; set; }
            public Dictionary<string, string> Details { get; set; } = new();
            public List<string> Warnings { get; set; } = new();
        }
        public class StartWorkflowOptions
        {
            public string CorrelationId { get; init; } 
            public  string TenantId { get; init; }
        }

        public class ApproveWorkflowResponse
        {
            public string WorkflowExecutionId { get; set; }
            public bool Approved { get; set; }
            public string Status { get; set; }
        public string SubStatus { get; set; }
        }
    public class CancelWorkflowResponse
    {
        public string WorkflowExecutionId { get; set; }
        public bool Cancelled { get; set; }
        public string Status { get; set; }
        public string SubStatus { get; set; }
    }
    public class ResumeWorkflowResponse
    {
        public string WorkflowExecutionId { get; set; }
        public string Task { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public string SubStatus { get; set; }
        public List<BookmarkResponseDto> Bookmark { get; set; }
    }
    public class DispatchWorkflowResponse
    {
        public string WorkflowInstanceId { get; set; }
    }
    public class WorkflowResumeResult
    {
        public string ExecutionId { get; set; }
        public string Status { get; set; }
        public string SubStatus { get; set; }
    }
    public class WorkflowDefinitionResponse
    {
        public string DefinitionId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Version { get; set; }
        public bool IsPublished { get; set; }
        public bool IsLatest { get; set; }
        public string? Description { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class WorkflowDefinitionPagedResponse
    {
        public List<WorkflowDefinitionResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    // ── StringData DTOs ────────────────────────────────────────────
    public class WorkflowStringDataDto
    {
            public string? Id { get; set; }

            public string? Type { get; set; }

            public int Version { get; set; }

            public Dictionary<string, JsonElement>? CustomProperties { get; set; }

            public Dictionary<string, JsonElement>? Metadata { get; set; }

            public List<JsonElement>? Activities { get; set; }

            public List<JsonElement>? Connections { get; set; }

            public JsonElement? Options { get; set; }

            public List<JsonElement>? Variables { get; set; }

            public List<JsonElement>? Inputs { get; set; }

            public List<JsonElement>? Outputs { get; set; }

            public List<JsonElement>? Outcomes { get; set; }
    }

    public class WorkflowOptionsDto
    {
        public bool AutoUpdateConsumingWorkflows { get; set; }
    }

    public class WorkflowVariableDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string? Value { get; set; }
        public string? StorageDriverTypeName { get; set; }
    }

    // ── Props DTOs ─────────────────────────────────────────────────
    public class WorkflowPropsDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Version { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }
        public List<WorkflowActivityDto>? Activities { get; set; }
        public List<WorkflowConnectionDto>? Connections { get; set; }
        public List<WorkflowVariableDto>? Variables { get; set; }
    }

    public class WorkflowActivityDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Name { get; set; }
        public int Version { get; set; }
        public Dictionary<string, object>? CustomProperties { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class WorkflowConnectionDto
    {
        public WorkflowConnectionEndpointDto Source { get; set; } = new();
        public WorkflowConnectionEndpointDto Target { get; set; } = new();
    }

    public class WorkflowConnectionEndpointDto
    {
        public string Activity { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
    }

    // ── New Full Response DTO (extends existing without modifying) ──
    public class WorkflowDefinitionDetailResponse : WorkflowDefinitionResponse
    {
        public bool? IsReadonly { get; set; }
        public bool? IsSystem { get; set; }
        public string? TenantId { get; set; }
        public JsonElement? StringData { get; set; }
        public WorkflowPropsDto? Props { get; set; }
    }

    // ── New Paged Response ─────────────────────────────────────────
    public class WorkflowDefinitionDetailPagedResponse
    {
        public List<WorkflowDefinitionDetailResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    
}
