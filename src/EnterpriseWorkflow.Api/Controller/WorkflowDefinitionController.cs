//using Microsoft.AspNetCore.Mvc;

//namespace EnterpriseWorkflow.Api.Controller
//{
//    public class WorkflowdefinitionController : Controller
//    {
//        public IActionResult Index()
//        {
//            return View();
//        }
//    }
//}


using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Entities;
using EnterpriseWorkflow.Application.Ports.Inbound;
using EnterpriseWorkflow.Application.Ports.Inbound.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.ValueObjects;
using EnterpriseWorkflow.Utilities.Observability;
using Microsoft.AspNetCore.Mvc;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Traces;
using System.Diagnostics;

namespace EnterpriseWorkflow.Api.Controllers;

[ApiController]
[Route("api/v1/definitions")]
public class WorkflowDefinitionController : ControllerBase
{
    private readonly IWorkflowDefinition _definitionService;
    private readonly ILoggingService _logger;
    private readonly INDTracerService _tracer;

    public WorkflowDefinitionController(IWorkflowDefinition definitionService,
        INDTracerService tracer,
        ILoggingService logger)
    {
        _definitionService = definitionService;
        _tracer = tracer;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Workflowdefinition request)
    {
        const string serviceName = "CreateWorkflowDefinition";

        var filterIds = new Dictionary<string, object>
        {
            ["DefinitionId"] = request?.DefinitionId ?? "NULL"
        };

        _logger.LogInformation(serviceName,
            "[Controller] Create Workflow Definition API Started",
            filterIds: filterIds);

        _tracer.AddEvent("create_workflow_definition_request_started");


        var ctx = BuildExecutionModel();
      
        var id = await _definitionService.CreateDefinitionAsync(request, ctx);

        _tracer.AddEvent("create_workflow_definition_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(serviceName,
            "[Controller] Create Workflow Definition API Ended",
            filterIds: filterIds);

        return CreatedAtAction(nameof(Get), new { id }, new { id });
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> Publish(string id)
    {
        const string serviceName = "PublishWorkflowDefinition";

        var filterIds = new Dictionary<string, object>
        {
            ["DefinitionId"] = id ?? "NULL"
        };

        _logger.LogInformation(serviceName,
            "[Controller] Publish Workflow Definition API Started",
            filterIds: filterIds);

        _tracer.AddEvent("publish_workflow_definition_request_started");

        var ctx = BuildExecutionModel();
        await _definitionService.PublishDefinitionAsync(id, ctx);

        _tracer.AddEvent("publish_workflow_definition_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(serviceName,
            "[Controller] Publish Workflow Definition API Ended",
            filterIds: filterIds);

        return Ok();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        const string serviceName = "GetWorkflowDefinition";

        var filterIds = new Dictionary<string, object>
        {
            ["DefinitionId"] = id ?? "NULL"
        };

        _logger.LogInformation(serviceName,
            "[Controller] Get Workflow Definition API Started",
            filterIds: filterIds);

        _tracer.AddEvent("get_workflow_definition_request_started");

        var ctx = BuildExecutionModel();
        var def = await _definitionService.GetDefinitionAsync(id, ctx);
        if (def == null) return NotFound();

        _tracer.AddEvent("get_workflow_definition_completed_successfully");
        _tracer.SetStatus(ActivityStatusCode.Ok);

        _logger.LogInformation(serviceName,
            "[Controller] Get Workflow Definition API Ended",
            filterIds: filterIds);

        return Ok(def);
    }

    [HttpPost("LoadWorkflowDefinitionFile")]
    public async Task<IActionResult> LoadFromFile([FromBody]  string FileName, CancellationToken ct)
    {
        var result = await _definitionService.LoadAndCreateDefinitionAsync(FileName, ct);
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

// Request DTOs
//public record CreateDefinitionRequest(string Name, string Version, List<StepDto> Steps);
//public record StepDto(string Name, int SeqNo, List<TaskDto> Tasks);
//public record TaskDto(string Name, int SeqNo);
