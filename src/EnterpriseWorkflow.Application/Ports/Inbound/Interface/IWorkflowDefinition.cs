using EnterpriseWorkflow.Application.UseCases.CreateDefinition;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Inbound.Interface;
public interface IWorkflowDefinition
{
    Task<string> CreateDefinitionAsync(Workflowdefinition definition, ExecutionModel ctx, CancellationToken ct = default);
    Task PublishDefinitionAsync(string definitionId, ExecutionModel ctx, CancellationToken ct = default);
    Task<Workflowdefinition?> GetDefinitionAsync(string id, ExecutionModel ctx, CancellationToken ct = default);
    Task<string> CreateDefinitionAsync(
                string json,
                CancellationToken ct = default);

    Task<string> LoadAndCreateDefinitionAsync(string FileName, CancellationToken ct = default);
}

