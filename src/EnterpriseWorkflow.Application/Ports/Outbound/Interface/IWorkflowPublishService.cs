using EnterpriseWorkflow.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Outbound.Interface
{
    public interface IWorkflowPublishService
    {
        Task<bool> PublishAsync(long workflowVersionId, CancellationToken ct);
    }
}
