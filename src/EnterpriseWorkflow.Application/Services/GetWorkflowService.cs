using EnterpriseWorkflow.Application.Model;
using EnterpriseWorkflow.Application.Ports.Inbound.Interface;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.Enums;
using EnterpriseWorkflow.Domain.ValueObjects;
using EnterpriseWorkflow.Utilities.Observability;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Traces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Services;

public class GetWorkflowService:IGetWorkflow
{
    private readonly IWorkflowEngineAdapter _engineAdapter;
    private readonly IAuthorizationHandler _auth;
    private readonly ILoggingService _logger;
    private readonly INDTracerService _tracer;


    public GetWorkflowService( IWorkflowEngineAdapter engineAdapter, IAuthorizationHandler auth,
        INDTracerService tracer,
        ILoggingService logger)
    {
        _engineAdapter = engineAdapter;
        _auth = auth;
        _tracer = tracer;
        _logger = logger;
    }
    public async Task<WorkflowExecutionState> GetWorkflowStatusAsync(string executionId, ExecutionModel ctx, CancellationToken ct = default)
    {
        await _auth.AuthorizeAsync(ctx, Permission.ManageDefinition, ct);
        // definition.Validate();
        _logger.LogInformation(
        "GetWorkflowStatusAsync",
        "[Service] Calling Workflow Engine");
        // Register with Elsa engine via adapter
        var result = await _engineAdapter.GetExecutionStateAsync(executionId, ct);

        _logger.LogInformation(
        "GetWorkflowStatusAsync",
        "[Service] Called Workflow Engine Successfully");
        return result;
        //definition.Publish(ctx.UserId);


        //return await _repo.SaveAsync(definition, ct);

    }
}
