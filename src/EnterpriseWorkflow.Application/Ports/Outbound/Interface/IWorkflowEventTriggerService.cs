using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Outbound.Interface
{
    // IWorkflowEventTriggerService.cs
    public interface IWorkflowEventTriggerService
    {
        Task<TriggerEventResponse> TriggerAsync(TriggerEventRequest request, ExecutionModel ctx, CancellationToken ct);
    }
}
