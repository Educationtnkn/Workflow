using EnterpriseWorkflow.Application.Model;
using EnterpriseWorkflow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Inbound.Interface;

public interface IGetWorkflow
{
    Task<WorkflowExecutionState> GetWorkflowStatusAsync(string executionId, ExecutionModel ctx, CancellationToken ct = default);
}
