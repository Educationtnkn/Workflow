using EnterpriseWorkflow.Domain.Enums;
using EnterpriseWorkflow.Domain.ValueObjects;


namespace EnterpriseWorkflow.Domain.Entities.Workflow;


public class WorkflowStep
{
    public string StepId { get;  set; } = Guid.NewGuid().ToString();
    public string? WorkflowDefinitionId { get; set; } = default!;
    public string? StepName { get;  set; } = default!;
    public StepType StepType { get;  set; } // Task, Wait, SubWorkflow, Action
    public int SeqNo { get;  set; }
    public string? CompensationStepId { get;  set; }
    public string? RetryPolicyId { get;  set; }
    public int? TimeoutMinutes { get;  set; }
    public string? RuleSetId { get; set; }

    //readonly List<WorkflowTask> _tasks = new();
    public List<WorkflowTask>? Tasks { get; set; }

    public static WorkflowStep Create(string stepName, StepType stepType, int seqNo)
    {
        return new WorkflowStep
        {
            StepName = stepName,
            StepType = stepType,
            SeqNo = seqNo
        };
    }

    public void AddTask(WorkflowTask task)
    {
        task.StepId = StepId;
        //_tasks.Add(task);
    }
}
//public class WorkflowStep
//{
//    public string Id { get;  set; } = Guid.NewGuid().ToString();
//    public required string StepName { get; init; }
//    public required int SeqNo { get; init; }
//    public string? CompensationStepId { get; init; }
//    public ICollection<WorkflowTask> Tasks { get; init; } = new List<WorkflowTask>();
//}