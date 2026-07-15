using EnterpriseWorkflow.Domain.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Storage.Contracts
{

    public interface IConfigNodeLookupRepository
    {
        Task<ConfigNodeResult?> GetByEngineActivityReferenceAsync(
            string engineActivityReference,
            CancellationToken ct);
    }
    public record ConfigNodeResult(long ConfigNodeId, string NodeTableType);
}
