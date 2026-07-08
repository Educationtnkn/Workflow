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
[Route("api/v1/Demo")]

public class DemoController : ControllerBase
{
    private readonly ILoggingService _logger;
    private readonly INDTracerService _tracer;

    public DemoController(IGetReportSummary reportService,
         INDTracerService tracer,
        ILoggingService logger)
    {
        _tracer = tracer;
        _logger = logger;
    }

        [HttpPost("InitiateCase")]
        public WorkflowDto InitiateCase(WorkflowDto request)
        {
            const string serviceName = "InitiateCase";

            var filterIds = new Dictionary<string, object>
            {
                ["EntityId"] = request?.Input?.entityId ?? "NULL",
                ["EntityType"] = request?.Input?.entityType ?? "NULL"
            };

            _logger.LogInformation(serviceName,
                "[External API] InitiateCase API Started",
                filterIds: filterIds);

            _tracer.AddEvent("initiate_case_request_started");

            Console.WriteLine("API Initiate Case for case id " + request.Input?.entityId);
            _logger.LogInformation(
                serviceName,
                $"[External API] API CreateTask for case id {request?.Input?.entityId} of type {request?.Input?.entityType}",
                filterIds: filterIds);


        _tracer.AddEvent("initiate_case_completed_successfully");
            _tracer.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(serviceName,
                "[External API] InitiateCase API Ended",
                filterIds: filterIds);

            return request;
        }

        [HttpPost("CreateTask")]
        public WorkflowDto CreateTask(WorkflowDto request)
        {
            const string serviceName = "CreateTask";

            var filterIds = new Dictionary<string, object>
            {
                ["EntityId"] = request?.Input?.entityId ?? "NULL",
                ["EntityType"] = request?.Input?.entityType ?? "NULL"
            };

            _logger.LogInformation(serviceName,
                "[External API] CreateTask API Started",
                filterIds: filterIds);

            _tracer.AddEvent("create_task_request_started");

            Console.WriteLine($"API CreateTask for case id {request.Input?.entityId} of type {request.Input?.entityType}");
            _logger.LogInformation(
                serviceName,
                $"[External API] API CreateTask for case id {request?.Input?.entityId} of type {request?.Input?.entityType}",
                filterIds: filterIds);

        _tracer.AddEvent("create_task_completed_successfully");
            _tracer.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(serviceName,
                "[External API] CreateTask API Ended",
                filterIds: filterIds);

            return request;
        }

        [HttpPost("AssignTask")]
        public WorkflowDto AssignTask(WorkflowDto request)
        {
            const string serviceName = "AssignTask";

            var filterIds = new Dictionary<string, object>
            {
                ["EntityId"] = request?.Input?.entityId ?? "NULL",
                ["EntityType"] = request?.Input?.entityType ?? "NULL"
            };

            _logger.LogInformation(serviceName,
                "[External API] AssignTask API Started",
                filterIds: filterIds);

            _tracer.AddEvent("assign_task_request_started");

            Console.WriteLine($"API AssignTask for case id {request.Input?.entityId} of type {request.Input?.entityType}");
                _logger.LogInformation(
                serviceName,
                $"[External API] API AssignTask for case id {request?.Input?.entityId} of type {request?.Input?.entityType}",
                filterIds: filterIds);


        _tracer.AddEvent("assign_task_completed_successfully");
            _tracer.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(serviceName,
                "[External API] AssignTask API Ended",
                filterIds: filterIds);

            return request;
        }

        [HttpPost("SetPriority")]
        public WorkflowDto SetPriority(WorkflowDto request)
        {
            const string serviceName = "SetPriority";

            var filterIds = new Dictionary<string, object>
            {
                ["EntityId"] = request?.Input?.entityId ?? "NULL",
                ["EntityType"] = request?.Input?.entityType ?? "NULL"
            };

            _logger.LogInformation(serviceName,
                "[External API] SetPriority API Started",
                filterIds: filterIds);

            _tracer.AddEvent("set_priority_request_started");

            Console.WriteLine($"API SetPriority for case id {request.Input?.entityId} of type {request.Input?.entityType}");

            _logger.LogInformation(
            serviceName,
            $"[External API] API SetPriority for case id {request?.Input?.entityId} of type {request?.Input?.entityType}",
            filterIds: filterIds);

        _tracer.AddEvent("set_priority_completed_successfully");
            _tracer.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(serviceName,
                "[External API] SetPriority API Ended",
                filterIds: filterIds);

            return request;
        }

        [HttpPost("UpdateStatus")]
        public WorkflowDto UpdateStatus(WorkflowDto request)
        {
            const string serviceName = "UpdateStatus";

            var filterIds = new Dictionary<string, object>
            {
                ["EntityId"] = request?.Input?.entityId ?? "NULL",
                ["EntityType"] = request?.Input?.entityType ?? "NULL"
            };

            _logger.LogInformation(serviceName,
                "[External API] UpdateStatus API Started",
                filterIds: filterIds);

            _tracer.AddEvent("update_status_request_started");

            Console.WriteLine($"API UpdateStatus for case id {request.Input?.entityId} of type {request.Input?.entityType}");

                _logger.LogInformation(
                serviceName,
                $"[External API] API UpdateStatus for case id {request?.Input?.entityId} of type {request?.Input?.entityType}",
                filterIds: filterIds);

        // call resume workflow or approve workflow
        // if signal is approve, resume

        _tracer.AddEvent("update_status_completed_successfully");
            _tracer.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(serviceName,
                "[External API] UpdateStatus API Ended",
                filterIds: filterIds);

            return request;
        }

        [HttpPost("FinalDisposition")]
        public WorkflowDto FinalDisposition(WorkflowDto request)
        {
            const string serviceName = "FinalDisposition";

            var filterIds = new Dictionary<string, object>
            {
                ["EntityId"] = request?.Input?.entityId ?? "NULL",
                ["EntityType"] = request?.Input?.entityType ?? "NULL"
            };

            _logger.LogInformation(serviceName,
                "[External API] FinalDisposition API Started",
                filterIds: filterIds);

            _tracer.AddEvent("final_disposition_request_started");

            Console.WriteLine($"API FinalDisposition for case id {request.Input?.entityId} of type {request.Input?.entityType}");


            _logger.LogInformation(
            serviceName,
            $"[External API] API FinalDisposition for case id {request?.Input?.entityId} of type {request?.Input?.entityType}",
            filterIds: filterIds);

        _tracer.AddEvent("final_disposition_completed_successfully");
            _tracer.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(serviceName,
                "[External API] FinalDisposition API Ended",
                filterIds: filterIds);

            return request;
        }

        [HttpPost("CloseCase")]
        public WorkflowDto CloseCase(WorkflowDto request)
        {
            const string serviceName = "CloseCase";

            var filterIds = new Dictionary<string, object>
            {
                ["EntityId"] = request?.Input?.entityId ?? "NULL",
                ["EntityType"] = request?.Input?.entityType ?? "NULL"
            };

            _logger.LogInformation(serviceName,
                "[External API] CloseCase API Started",
                filterIds: filterIds);

            _tracer.AddEvent("close_case_request_started");

            Console.WriteLine($"API CloseCase for case id {request.Input?.entityId} of type {request.Input?.entityType}");

            _logger.LogInformation(
            serviceName,
            $"[External API] API CloseCase for case id {request?.Input?.entityId} of type {request?.Input?.entityType}",
            filterIds: filterIds);

        _tracer.AddEvent("close_case_completed_successfully");
            _tracer.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(serviceName,
                "[External API] CloseCase API Ended",
                filterIds: filterIds);

            return request;
        }

        [HttpPost("ConsolidateFindings")]
        public WorkflowDto ConsolidateFindings(WorkflowDto request)
        {
            const string serviceName = "ConsolidateFindings";

            var filterIds = new Dictionary<string, object>
            {
                ["EntityId"] = request?.Input?.entityId ?? "NULL",
                ["EntityType"] = request?.Input?.entityType ?? "NULL"
            };

            _logger.LogInformation(serviceName,
                "[External API] ConsolidateFindings API Started",
                filterIds: filterIds);

            _tracer.AddEvent("consolidate_findings_request_started");

            Console.WriteLine($"API ConsolidateFindings for case id {request.Input?.entityId} of type {request.Input?.entityType}");

            _logger.LogInformation(
            serviceName,
            $"[External API] API ConsolidateFindings for case id {request?.Input?.entityId} of type {request?.Input?.entityType}",
            filterIds: filterIds);

        _tracer.AddEvent("consolidate_findings_completed_successfully");
            _tracer.SetStatus(ActivityStatusCode.Ok);

            _logger.LogInformation(serviceName,
                "[External API] ConsolidateFindings API Ended",
                filterIds: filterIds);

            return request;
        }
}
