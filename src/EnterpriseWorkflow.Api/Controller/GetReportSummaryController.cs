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
[Route("api/v1/Workflow")]

public class GetReportSummaryController : ControllerBase
{
    private readonly IGetReportSummary _reportService;
    private readonly ILoggingService _logger;
    private readonly INDTracerService _tracer;

    public GetReportSummaryController(IGetReportSummary reportService,
         INDTracerService tracer,
        ILoggingService logger)
    {
        _reportService = reportService;
        _tracer = tracer;
        _logger = logger;
    }

    [HttpPost("getWorkflowDefinition")]
    public async Task<IActionResult> GetWorkflowDefinitions(
    [FromBody] WorkflowDefinitionQueryRequest request,
    CancellationToken cancellationToken)
    {
        const string serviceName = "GetWorkflowDefinitions";

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowDefinitions API Started");

        _tracer.AddEvent("get_workflow_definitions_request_started");

        var ctx = BuildExecutionModel();
        var result = await _reportService
            .GetWorkflowDefinitionsAsync(request, ctx,cancellationToken);

        _tracer.AddEvent("get_workflow_definitions_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowDefinitions API Ended");

        return Ok(result);
    }
    [HttpPost("getWorkflowDefinition/detail")]
    public async Task<IActionResult> GetWorkflowDefinitionsDetail(
    [FromBody] WorkflowDefinitionQueryRequest request,
    CancellationToken cancellationToken)
    {

        const string serviceName = "GetWorkflowDefinitionsDetail";

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowDefinitionsDetail API Started");

        _tracer.AddEvent("get_workflow_definitions_detail_request_started");

        var ctx = BuildExecutionModel();
        var result = await _reportService
            .GetWorkflowDefinitionsDetailAsync(request, ctx, cancellationToken);

        _tracer.AddEvent("get_workflow_definitions_detail_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowDefinitionsDetail API Ended");

        return Ok(result);
    }
    [HttpPost("getWorkflowInstance{ExecutionId}/Activity")]
    public async Task<IActionResult> GetWorkflowInstanceActivity(
    [FromRoute] string ExecutionId,
    [FromBody] WorkflowDefinitionQueryRequest request,
    CancellationToken cancellationToken)
    {

        const string serviceName = "GetWorkflowInstanceActivity";

        var filterIds = new Dictionary<string, object>
        {
            ["WorkflowExecutionId"] = ExecutionId ?? "NULL"
        };

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowInstanceActivity API Started",
            filterIds: filterIds);

        _tracer.AddEvent("get_workflow_instance_activity_request_started");

        var ctx = BuildExecutionModel();
        var result = await _reportService
            .GetWorkflowInstanceActivityAsync(request, ExecutionId, ctx, cancellationToken);

        _tracer.AddEvent("get_workflow_instance_activity_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowInstanceActivity API Ended",
            filterIds: filterIds);

        return Ok(result);
    }
    [HttpPost("getWorkflowInstance{ExecutionId}/Activity/detail")]
    public async Task<IActionResult> GetWorkflowInstanceActivityDetail(
    [FromRoute] string ExecutionId,
    [FromBody] WorkflowDefinitionQueryRequest request,
    CancellationToken cancellationToken)
    {
        const string serviceName = "GetWorkflowInstanceActivityDetail";

        var filterIds = new Dictionary<string, object>
        {
            ["WorkflowExecutionId"] = ExecutionId ?? "NULL"
        };

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowInstanceActivityDetail API Started",
            filterIds: filterIds);

        _tracer.AddEvent("get_workflow_instance_activity_detail_request_started");

        var ctx = BuildExecutionModel();
        var result = await _reportService
            .GetWorkflowInstanceActivityDetailAsync(request, ExecutionId, ctx, cancellationToken);
        _tracer.AddEvent("get_workflow_instance_activity_detail_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowInstanceActivityDetail API Ended",
            filterIds: filterIds);

        return Ok(result);
    }
    [HttpPost("getWorkflowDefinition{DefinitionId}/Activity")]
    public async Task<IActionResult> GetWorkflowDefinitionsActivity(
    [FromRoute] string DefinitionId,
    [FromBody] WorkflowDefinitionQueryRequest request,
    CancellationToken cancellationToken)
    {
        const string serviceName = "GetWorkflowDefinitionsActivity";

        var filterIds = new Dictionary<string, object>
        {
            ["DefinitionId"] = DefinitionId ?? "NULL"
        };

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowDefinitionsActivity API Started",
            filterIds: filterIds);

        _tracer.AddEvent("get_workflow_definitions_activity_request_started");

        var ctx = BuildExecutionModel();
        var result = await _reportService
            .GetLastExecutedWorkflowActivityAsync(request, DefinitionId, ctx, cancellationToken);

        _tracer.AddEvent("get_workflow_definitions_activity_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowDefinitionsActivity API Ended",
            filterIds: filterIds);

        return Ok(result);
    }
    [HttpPost("getWorkflowDefinition{DefinitionId}/Activity/detail")]
    public async Task<IActionResult> GetWorkflowDefinitionsActivityDetail(
    [FromRoute] string DefinitionId,
    [FromBody] WorkflowDefinitionQueryRequest request,
    CancellationToken cancellationToken)
    {
        const string serviceName = "GetWorkflowDefinitionsActivityDetail";

        var filterIds = new Dictionary<string, object>
        {
            ["DefinitionId"] = DefinitionId ?? "NULL"
        };

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowDefinitionsActivityDetail API Started",
            filterIds: filterIds);

        _tracer.AddEvent("get_workflow_definitions_activity_detail_request_started");

        var ctx = BuildExecutionModel();
        var result = await _reportService
            .GetLastExecutedWorkflowActivityDetailAsync(request, DefinitionId, ctx, cancellationToken);

        _tracer.AddEvent("get_workflow_definitions_activity_detail_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(
            serviceName,
            "[Controller] GetWorkflowDefinitionsActivityDetail API Ended",
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
