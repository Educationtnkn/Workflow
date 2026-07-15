namespace EnterpriseWorkflow.Domain.Entities.Workflow;

//public class WorkflowTask
//{
//    public string Id { get;  set; } = Guid.NewGuid().ToString();
//    public required string TaskName { get; init; }
//    public int SeqNo { get; init; }
//}

public class WorkflowTask
{
    public string Id { get;  set; } = Guid.NewGuid().ToString();
    public string StepId { get; set; } = default!;
    public string TaskName { get;  set; } = default!;
    public string TaskType { get;  set; } = default!; // "Approval", "Notification", "HttpCall"
    public int SeqNo { get;  set; }
    public bool? IsConditional { get;  set; }
    public string? RuleSetId { get;  set; }

    //readonly List<WorkflowAction> _actions = new();
    public List<WorkflowAction> Actions { get; set; }

    public static WorkflowTask Create(string taskName, string taskType, int seqNo)
    {
        return new WorkflowTask
        {
            TaskName = taskName,
            TaskType = taskType,
            SeqNo = seqNo
        };
    }

    public void AddAction(WorkflowAction action)
    {
        action.TaskId = Id;
        //_actions.Add(action);
    }
}