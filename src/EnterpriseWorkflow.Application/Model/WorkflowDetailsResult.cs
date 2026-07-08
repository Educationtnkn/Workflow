using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Model
{
    public class ActivityExecutionBriefResponse
    {
        public string ActivityId { get; set; } = default!;
        public string ActivityType { get; set; } = default!;
        public string Status { get; set; } = default!;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public object? Output { get; set; }
    }
    public class ActivityExecutionBriefPagedResponse
    {
        public List<ActivityExecutionBriefResponse> Items { get; set; } = new();

        public int TotalCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

    }
    public class ActivityExecutionDetailResponse
    {
        public string Id { get; set; } = string.Empty;
        public string WorkflowInstanceId { get; set; } = string.Empty;
        public string ActivityId { get; set; } = string.Empty;
        public string ActivityNodeId { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public int ActivityTypeVersion { get; set; }
        public string? ActivityName { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool HasBookmarks { get; set; }
        public object? Payload { get; set; }
        public object? Outputs { get; set; }
        public object? Properties { get; set; }
        public string? Exception { get; set; }
        public DateTimeOffset StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }

    public class ActivityExecutionDetailPagedResponse
    {
        public List<ActivityExecutionDetailResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class LastExecutedWorkflowActivityResponse
    {
        public string DefinitionId { get; set; } = string.Empty;
        public string WorkflowInstanceId { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public ActivityExecutionBriefPagedResponse Activities { get; set; } = new();
    }
    public class LastExecutedWorkflowActivityDetailResponse
    {
        public string DefinitionId { get; set; } = string.Empty;
        public string WorkflowInstanceId { get; set; } = string.Empty;
        public DateTimeOffset CreatedAt { get; set; }
        public ActivityExecutionDetailPagedResponse Activities { get; set; } = new();
    }
}
