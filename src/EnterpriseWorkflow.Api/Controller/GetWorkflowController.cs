using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Entities;
using EnterpriseWorkflow.Application.Ports.Inbound;
using EnterpriseWorkflow.Application.Ports.Inbound.Interface;
using EnterpriseWorkflow.Application.Services;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.ValueObjects;
using EnterpriseWorkflow.Utilities.Observability;
using Microsoft.AspNetCore.Mvc;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Traces;
using System.Diagnostics;
namespace EnterpriseWorkflow.Api.Controller;


[ApiController]
[Route("api/v1/getWorkflow")]
public class GetWorkflowController : ControllerBase
{
    private readonly IGetWorkflow _getWorkflowService;
    private readonly ILoggingService _logger;
    private readonly INDTracerService _tracer;

    public GetWorkflowController(IGetWorkflow getWorkflow,
         INDTracerService tracer,
        ILoggingService logger)
    {
        _getWorkflowService = getWorkflow;
        _tracer = tracer;
        _logger = logger;
    }

    
    [HttpGet("Status/{executionId}")]
    public async Task<IActionResult> GetWorkflowStatus(string executionId)
    {
        const string serviceName = "GetWorkflowStatus";

        var filterIds = new Dictionary<string, object>
        {
            ["WorkflowExecutionId"] = executionId ?? "NULL"
        };

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowStatus API Started",
            filterIds: filterIds);

        _tracer.AddEvent("get_workflow_status_request_started");

        var ctx = BuildExecutionModel();

        var result = await _getWorkflowService.GetWorkflowStatusAsync(executionId, ctx);

        _tracer.AddEvent("get_workflow_status_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowStatus API Ended",
            filterIds: filterIds);

        return Ok(result);
    }
    private ExecutionModel BuildExecutionModel()
    {
        // In real app, extract from JWT or headers
        return new ExecutionModel
        {
            TenantId = HttpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? "default",
            DomainId = HttpContext.Request.Headers["X-Domain-Id"].FirstOrDefault() ?? string.Empty,
            UserId = User.Identity?.Name ?? "system",
            CorrelationId = HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault() ?? Guid.NewGuid().ToString(),
            Principal = User
        };
    }
}
