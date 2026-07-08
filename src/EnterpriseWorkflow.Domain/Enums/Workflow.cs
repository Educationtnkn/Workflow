using System.Text.Json.Serialization;

namespace EnterpriseWorkflow.Domain.Enums;

public enum StepType
{
    Task,
    Wait,
    SubWorkflow,
    Parallel,
    Decision
}

public enum WorkflowStatus
{
    Draft,
    Published,
    Archived,
    Running,
    Suspended,
    Finished,
    Faulted,
    Cancelled
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkflowExecutionStatus
{
    Running,
    Suspended,
    Completed,
    Failed,
    Cancelled
}

public enum ActivityStatus
{
    Pending,
    Running,
    Completed,
    Faulted,
    Skipped
}

public enum WorkflowStatusFilter
{
    All,
    Running,
    Suspended,
    Completed,
    Failed,
    Cancelled
}
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WorkflowExecutionSubStatus
{
    Running,
    Suspended,
    Completed,
    Failed,
    Cancelled
}