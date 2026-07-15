using EnterpriseWorkflow.Domain.ValueObjects;
using System.Text.Json;

namespace EnterpriseWorkflow.Domain.Entities.Workflow;


public class WorkflowConst
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WorkflowDefinitionId { get; set; }
    public string StepName { get; set; }
    public string StepType { get; set; }  // Task, Action, Gateway, etc.
    public int SeqNo { get; set; }
    public string? ConfigJson { get; set; } = "{}";

}
public class Workflowdefinition
{
    public string DefinitionId { get; set; }
    public string? ParentWorkflowDefinitionId { get; set; }
    public string WorkflowName { get; set; }
    public string Description { get; set; }

    public string TenantId { get; set; }

    public string DomainId { get; set; }
    public bool IsActive { get; set; }

    // Navigation properties
    public WorkflowVersion Version { get; set; }
    public List<WorkflowTransition> Transitions { get; set; } = new();

    public List<WorkflowStep> Steps { get; set; } = new();
    public List<TaskDefinition> Tasks { get; set; } = new();
    public List<ActionDefinition> Actions { get; set; } = new();
    // ... other needed collections
    public int VersionNo { get; set; }
    public bool IsPublished { get; set; }
    public string? CronExpression { get; set; }
}

public class WorkflowVersion
{
    public int WorkflowVersionId { get; set; }
    public string WorkflowDefinitionId { get; set; }
    public string? ParentWorkflowVersionId { get; set; }
    public int TenantId { get; set; }
    public int DomainId { get; set; }
    public int VersionNo { get; set; }
    public string Status { get; set; }  // Draft, Published, etc.
    public bool IsPublished { get; set; }
    public bool IsCurrent { get; set; }
}

// TaskDefinition table
public class TaskDefinition
{
    public int TaskDefinitionId { get; set; }
    public int? ParentTaskDefinitionId { get; set; }
    public int TenantId { get; set; }
    public int DomainId { get; set; }
    public string TaskName { get; set; }
    public string Description { get; set; }
    public string TaskType { get; set; }   // System, Human, Gateway, Action Task, Human Action Hub
    public string TaskLevel { get; set; }  // Standard
    public string RuleReferenceType { get; set; } // NONE, RULE, RULE_SET
    public string? RuleReferenceId { get; set; }
    public int? EventDefinitionId { get; set; }
    public int? ExecutionPolicyId { get; set; }
    public bool IsActive { get; set; }

    public List<WorkflowAction> Actions { get; set; } = new();
}

// ActionDefinition table
public class ActionDefinition
{
    public int ActionDefinitionId { get; set; }
    public int TenantId { get; set; }
    public int DomainId { get; set; }
    public string ActionName { get; set; }
    public string Description { get; set; }
    public string ActionType { get; set; }   // UserAction, SystemAction
    public int? EventDefinitionId { get; set; }
    public bool IsActive { get; set; }
}

//public class WorkflowAction
//{
//    public string Id { get; set; }
//    public int SeqNo { get; set; }
//    public string ActionType { get; set; }    // RunJavaScript, ExecuteWorkflow, FlowDecision, etc.
//    public object Config { get; set; }        // typed config object
//}       



// WorkflowTransition table (critical for flow)
public class WorkflowTransition
{
    public int TransitionId { get; set; }
    public int WorkflowVersionId { get; set; }
    public string FromNodeType { get; set; }  // WORKFLOW, TASK, ACTION
    public int FromNodeId { get; set; }
    public string ToNodeType { get; set; }
    public int ToNodeId { get; set; }
    public string TransitionCode { get; set; }
    public string TransitionName { get; set; }
    public int Priority { get; set; }
    public string TransitionMode { get; set; }  // Structure, Sequential, ParallelSplit, ParallelJoin, EventWait
    public string TransitionPathType { get; set; } // Contains, Success, Custom, Event
    public string RuleReferenceType { get; set; } // NONE, RULE
    public int? RuleReferenceId { get; set; }
    public string BranchCompletionPolicy { get; set; } // All, Any, etc.
    public int? RequiredCompletionCount { get; set; }
    public int? EventDefinitionId { get; set; }
    public string DelayDuration { get; set; }
    public bool IsDefaultPath { get; set; }
    public bool IsActive { get; set; }
}

public class DynamicWorkflowRequest
{
    public string json { get; set; }
}

// Supporting DTOs for action configurations (same as before)
//public class RunJavaScriptConfig { public string Script { get; set; } }
//public class ExecuteWorkflowConfig
//{
//    public string WorkflowDefinitionId { get; set; }
//    public string InputTypeName { get; set; }
//    public string InputExpression { get; set; }
//    public bool WaitForCompletion { get; set; }
//}
//public class HttpConfig
//{
//    public string Url { get; set; }
//    public string Method { get; set; }
//    public object Body { get; set; }
//    public string ContentType { get; set; }
//    public int MaxRetries { get; set; }
//    public string RetryDelay { get; set; }
//}
//public class SetVariableConfig
//{
//    public string VariableId { get; set; }
//    public string VariableName { get; set; }
//    public string VariableTypeName { get; set; }
//    public TypedValue Value { get; set; }
//}
//public class WriteLineConfig { public TypedValue Text { get; set; } }
//public class WaitForEventConfig { public string EventName { get; set; } }
//public class FlowDecisionConfig { public string ConditionExpression { get; set; } }
//public class EvaluateRuleConfig
//{
//    public string RuleWorkflow { get; set; }
//    public string OutputVariable { get; set; }
//    public bool WaitForCompletion { get; set; }
//}
//public class TypedValue
//{
//    public ExpressionType ExprType { get; set; }
//    public string Value { get; set; }
//}
//public enum ExpressionType { Literal, JavaScript }
//}

//public  class Workflowdefinition
//{
//    public string Id { get;  set; } = Guid.NewGuid().ToString();
//    public string DefinitionId { get;  set; } = default!; // Stable logical ID
//    public string TenantId { get;  set; } = default!;
//    public string DomainId { get;  set; } = default!;
//    public string Name { get;  set; } = default!;
//    public string? Description { get;  set; }
//    public int Version { get;  set; } = 1;
//    public bool IsPublished { get;  set; }
//    public string? EngineType { get;  set; } = "Elsa"; // "Elsa", "Custom", "Temporal"
//    public DateTimeOffset? CreatedAt { get;  set; }
//    public string? CreatedBy { get;  set; } = default!;

//    // Your relational steps (ENTERPRISE MODEL - not Elsa format)
//     //readonly List<WorkflowStep>? _steps = new();
//    public IReadOnlyList<WorkflowStep>? Steps { get; set; }
//    // Navigation
//    public string? ElsaDefinitionId { get;  set; } // Store Elsa's internal ID after conversion

//    public static Workflowdefinition Create(string definitionId, string name, string tenantId, string domainId, string createdBy)
//    {
//        return new Workflowdefinition
//        {
//            DefinitionId = definitionId,
//            Name = name,
//            TenantId = tenantId,
//            DomainId = domainId,
//            CreatedBy = createdBy,
//            CreatedAt = DateTimeOffset.UtcNow,
//            EngineType = "Elsa"
//        };
//    }

//    public void AddStep(WorkflowStep step)
//    {
//        step.WorkflowDefinitionId = Id;
//        //_steps.Add(step);
//    }

//    public void Publish(string userId)
//    {
//        IsPublished = true;
//    }

//    public void SetElsaDefinitionId(string elsaDefinitionId)
//    {
//        ElsaDefinitionId = elsaDefinitionId;
//    }
//}
//public class Workflowdefinition
//{
//    public string Id { get;  set; } = Guid.NewGuid().ToString();
//    public required string TenantId { get; init; }
//    public required string DomainId { get; init; }
//    public required string WorkflowName { get; init; }
//    public required string Version { get; init; }
//    public required string EngineType { get; init; } = "Elsa";
//    public bool IsPublished { get;  set; }
//    public string? CreatedBy { get; init; }
//    public DateTimeOffset CreatedAt { get;  set; } = DateTimeOffset.UtcNow;
//    public string? PublishedBy { get;  set; }
//    public DateTimeOffset? PublishedAt { get;  set; }
//    public ICollection<WorkflowStep> Steps { get; init; } = new List<WorkflowStep>();

//    public void Publish(string userId)
//    {
//        IsPublished = true;
//        PublishedBy = userId;
//        PublishedAt = DateTimeOffset.UtcNow;
//    }

//    public void Validate()
//    {
//        if (string.IsNullOrWhiteSpace(WorkflowName))
//            throw new InvalidOperationException("Workflow name is required");
//        if (Steps == null || !Steps.Any())
//            throw new InvalidOperationException("At least one step is required");
//    }
//}