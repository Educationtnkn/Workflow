using EnterpriseWorkflow.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EnterpriseWorkflow.Application.Ports.Outbound.Implemenation.WorkFlowEngine;


namespace EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface
{
    public interface IBuildPublishElsaDefinition
    {
        Task<WorkflowPublishResult> BuildElsaDefinition(WorkflowPublishRequest request, CancellationToken ct);
    }


  
}
