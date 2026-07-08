using EnterpriseWorkflow.Domain.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Outbound.Interface;

public interface IWaitStateRepository
{

    Task<long> InsertAsync(WorkflowWaitState e, CancellationToken ct);
    Task<WorkflowWaitState?> GetActiveByCorrelationKeyAsync(string correlationKey, CancellationToken ct);

    Task MarkResumedAsync(long waitStateId, CancellationToken ct);
}
