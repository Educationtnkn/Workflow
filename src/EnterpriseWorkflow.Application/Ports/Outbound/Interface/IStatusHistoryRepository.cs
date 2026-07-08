using EnterpriseWorkflow.Domain.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Outbound.Interface;

public interface IStatusHistoryRepository
{
    Task<long> InsertAsync(StatusHistory e, CancellationToken ct);
    Task<int> NextSequenceAsync(long workflowInstanceId, CancellationToken ct);
}
