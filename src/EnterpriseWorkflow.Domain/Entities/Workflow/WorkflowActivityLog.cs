using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Domain.Entities.Workflow;

public class WorkflowActionLog
{
    public long Id { get; set; }
    public string WorkflowInstanceId { get; set; } = default!;
    public string NodeType { get; set; } = default!;
    public string ActionId { get; set; } = default!;
    public string ActionName { get; set; } = default!;
    public string ActionType { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string? BusinessMessage { get; set; }
    public string EngineStatus { get; set; } = default!;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public bool HasBookmarks { get; set; }
    public string? TenantId { get; set; }
    public string? SerializedException { get; set; }
    public DateTime LoggedAt { get; set; } = DateTime.UtcNow;
}
public class ActionStatusConfiguration
{
    public string WorkflowDefinitionId { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public List<ActionStatusMapping> ActionStatusMappings { get; set; } = new();
}

public class ActionStatusMapping
{
    public string ActionId { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;

    public Dictionary<string, string> StatusMappings { get; set; } = new();
}

