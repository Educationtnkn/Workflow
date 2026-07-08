using EnterpriseWorkflow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.UseCases.PublishDefinition
{
    // PublishWorkflowDefinitionCommand.cs
    public class PublishWorkflowDefinitionCommand
    {
        public string DefinitionId { get; set; } = default!;
        public int Version { get; set; }
        public ExecutionModel CallerContext { get; set; } = default!;
    }
}
