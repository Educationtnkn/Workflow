using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.Enums;
using EnterpriseWorkflow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.UseCases.CreateDefinition
{
    public class WorkflowdefinitionDto
    {
        public string DefinitionId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public string TenantId { get; set; } = default!;
        public string DomainId { get; set; } = default!;
        public List<WorkflowStepDto> Steps { get; set; } = new();
        public ExecutionModel CallerContext { get; set; } = default!;
    }

    public class WorkflowStepDto
    {
        public string StepName { get; set; } = default!;
        public StepType StepType { get; set; }
        public int SeqNo { get; set; }
        public List<WorkflowTaskDto> Tasks { get; set; } = new();
    }

    public sealed class WorkflowTaskDto
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string StepId { get; set; } = default!;
        public string TaskName { get; private set; } = default!;
        public string TaskType { get; private set; } = default!; // "Approval", "Notification", "HttpCall"
        public int SeqNo { get; private set; }
        public bool IsConditional { get; private set; }
        public string? RuleSetId { get; private set; }

        private readonly List<WorkflowActionDto> _actions = new();
        public IReadOnlyList<WorkflowActionDto> Actions => _actions.AsReadOnly();

        public static WorkflowTaskDto Create(string taskName, string taskType, int seqNo)
        {
            return new WorkflowTaskDto
            {
                TaskName = taskName,
                TaskType = taskType,
                SeqNo = seqNo
            };
        }

        public void AddAction(WorkflowActionDto action)
        {
            action.TaskId = Id;
            _actions.Add(action);
        }
    }

    public sealed class WorkflowActionDto
    {
        public string Id { get; private set; } = Guid.NewGuid().ToString();
        public string TaskId { get; set; } = default!;
        public string ActionName { get; private set; } = default!;
        public string ActionType { get; private set; } = default!;
        public string ActionCategory { get; private set; } = default!;
        public int SeqNo { get; private set; }
        public bool IsConditional { get; private set; }
        public string? RuleSetId { get; private set; }
        public bool IsDynamic { get; private set; }
        public string? DynamicRegistrationId { get; private set; }

        // Configuration as JSON string
        public string ConfigJson { get; private set; } = "{}";

        public static WorkflowActionDto Create(string actionName, string actionType, int seqNo, object config)
        {
            return new WorkflowActionDto
            {
                ActionName = actionName,
                ActionType = actionType,
                SeqNo = seqNo,
                ConfigJson = JsonSerializer.Serialize(config)
            };
        }
    }

}
