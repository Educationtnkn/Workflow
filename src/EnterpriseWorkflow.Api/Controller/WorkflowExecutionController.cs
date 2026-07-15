using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Entities;
using EnterpriseWorkflow.Application.Ports.Inbound;
using EnterpriseWorkflow.Application.Ports.Inbound.Interface;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Application.Services;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.ValueObjects;
using EnterpriseWorkflow.Utilities.Observability;
using Microsoft.AspNetCore.Mvc;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Traces;
using System.Diagnostics;

namespace EnterpriseWorkflow.Api.Controller;

[ApiController]
[Route("api/v1/executions")]
public class WorkflowExecutionController : ControllerBase
{
    private readonly IWorkflowPublishService _buildPublishElsa;
    private readonly IWorkflowExecution _executionService;
    private readonly ILoggingService _logger;
    private readonly INDTracerService _tracer;
    private readonly IWorkflowEventTriggerService _eventTrigger;

    public WorkflowExecutionController(IWorkflowExecution exceutionService, 
        INDTracerService tracer,
        ILoggingService logger, IWorkflowPublishService buildPublishElsa, IWorkflowEventTriggerService eventTrigger )
    {
        _executionService = exceutionService;
        _tracer = tracer;
        _logger = logger;
        _buildPublishElsa = buildPublishElsa;
        _eventTrigger = eventTrigger;
    }

    [HttpPost("exceuteWorkflow")]
    public async Task<IActionResult> Execute([FromBody] StartWorkflowRequest request, CancellationToken ct)
    {
        const string serviceName = "ExecuteWorkflow";

        var filterIds = new Dictionary<string, object>
        {
            ["DefinitionId"] = request.ElsaWorkflowDefinitionId ?? "NULL"
        };

        _logger.LogInformation(serviceName, "[Controller] ExecuteWorkflow API Started", filterIds: filterIds);
        _tracer.AddEvent("exceuteWorkflow_request_started");

        var result = await _executionService.StartWorkflowAsync(request, ct);

        if (!string.Equals(result.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
        {
            _tracer.AddEvent("exceuteWorkflow_completed_successfully");
            _tracer.SetStatus(ActivityStatusCode.Ok);
        }
        else
        {
            _tracer.AddEvent("exceuteWorkflow_rejected");
            _tracer.SetStatus(ActivityStatusCode.Error, result.ErrorMessage);
        }

        _logger.LogInformation(serviceName, "[Controller] ExecuteWorkflow API Ended", filterIds: filterIds);

        return Ok(result);
    }
    [HttpPost("ApproveWorkflow")]
    public async Task<IActionResult> ApproveWorkflow([FromBody] WorkflowAprroveRequest request,CancellationToken cancellationToken)
    {
        const string serviceName = "ApproveWorkflow";

        //var filterIds = new Dictionary<string, object>
        //{
        //    ["DefinitionId"] = request.WorkflowversionId ?? "NULL"
        //};

        //_logger.LogInformation(serviceName, "[Controller] ApproveWorkflow API Started", filterIds: filterIds);
        //_tracer.AddEvent("approveWorkflow_request_started");
        //var ctx = BuildExecutionModel();

        var result = await _buildPublishElsa.PublishAsync(request.WorkflowversionId, cancellationToken);

        _tracer.AddEvent("approveWorkflow_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);
      //  _logger.LogInformation(serviceName, "[Controller] ApproveWorkflow API Ended", filterIds: filterIds);

        return Ok(result);
    }

    [HttpPost("events/trigger")]
    public async Task<IActionResult> TriggerEvent([FromBody] TriggerEventRequest request, CancellationToken ct)
    {
        const string serviceName = "TriggerEvent";
        var filterIds = new Dictionary<string, object>
        {
            ["EventCode"] = request.EventCode,
            ["WorkflowInstanceNumber"] = request.WorkflowInstanceNumber
        };

        _logger.LogInformation(serviceName, "[Controller] TriggerEvent API Started", filterIds: filterIds);

        var ctx = BuildExecutionModel();
        var result = await _eventTrigger.TriggerAsync(request, ctx, ct);

        _tracer.AddEvent("trigger_event_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);
        _logger.LogInformation(serviceName, "[Controller] TriggerEvent API Ended", filterIds: filterIds);

        return Ok(result);
    }
    //[HttpPost("CancelWorkflow")]
    //public async Task<IActionResult> CancelWorkflow([FromBody] WorkflowCancelRequest request)
    //{
    //    const string serviceName = "CancelWorkflow";

    //    var filterIds = new Dictionary<string, object>
    //    {
    //        ["DefinitionId"] = request.workflowExecutionId ?? "NULL"
    //    };

    //    _logger.LogInformation(serviceName, "[Controller] CancelWorkflow API Started", filterIds: filterIds);
    //    _tracer.AddEvent("cancelWorkflow_request_started");
    //    var ctx = BuildExecutionModel();

    //    var result = await _executionService.CancelWorkflowAsync(request, ctx);

    //    _tracer.AddEvent("cancelWorkflow_completed_successfully");
    //    _tracer.SetStatus(ActivityStatusCode.Ok);
    //    _logger.LogInformation(serviceName, "[Controller] CancelWorkflow API Ended", filterIds: filterIds);

    //    return Ok(result);
    //}
    //[HttpPost("ResumeWorkflowActivity")]
    //public async Task<IActionResult> ResumeWorkflow([FromBody] WorkflowResumeRequest request)
    //{
    //    const string serviceName = "ResumeWorkflow";

    //    var filterIds = new Dictionary<string, object>
    //    {
    //        ["DefinitionId"] = request.workflowExecutionId ?? "NULL"
    //    };

    //    _logger.LogInformation(serviceName, "[Controller] ResumeWorkflow API Started", filterIds: filterIds);
    //    _tracer.AddEvent("resumeWorkflow_request_started");
    //    var ctx = BuildExecutionModel();

    //    var result = await _executionService.ResumeWorkflowAsync(request, ctx);

    //    _tracer.AddEvent("resumeWorkflow_completed_successfully");
    //    _tracer.SetStatus(ActivityStatusCode.Ok);
    //    _logger.LogInformation(serviceName, "[Controller] ResumeWorkflow API Ended", filterIds: filterIds);

    //    return Ok(result);
    //}

    //[HttpPost("DispatchWorkflow")]
    //public async Task<IActionResult> DispatchWorkflow([FromBody] WorkflowExecution request)
    //{
    //    const string serviceName = "DispatchWorkflow";

    //    var filterIds = new Dictionary<string, object>
    //    {
    //        ["DefinitionId"] = request.DefinitionId ?? "NULL"
    //    };

    //    _logger.LogInformation(serviceName, "[Controller] DispatchWorkflow API Started", filterIds: filterIds);
    //    _tracer.AddEvent("dispatch_Workflow_request_started");
    //    var ctx = BuildExecutionModel();

    //    var result = await _executionService.DispatchWorkflowAsync(request, ctx);

    //    _tracer.AddEvent("dispatch_completed_successfully");
    //    _tracer.SetStatus(ActivityStatusCode.Ok);
    //    _logger.LogInformation(serviceName, "[Controller] DispatchWorkflow API Ended", filterIds: filterIds);

    //    return Ok(result);
    //}

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

