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

public class GetReportSummaryService : IGetReportSummary
{
    private readonly IWorkflowEngineAdapter _engineAdapter;
    private readonly IAuthorizationHandler _auth;
    private readonly ILoggingService _logger;
    private readonly INDTracerService _tracer;

    public GetReportSummaryService(IWorkflowEngineAdapter engineAdapter, IAuthorizationHandler auth,
        INDTracerService tracer,
        ILoggingService logger)
    {
        _engineAdapter = engineAdapter;
        _auth = auth;
        _tracer = tracer;
        _logger = logger;
    }
    public async Task<WorkflowDefinitionPagedResponse> GetWorkflowDefinitionsAsync(WorkflowDefinitionQueryRequest request, ExecutionModel ctx, CancellationToken ct = default)
    {
        await _auth.AuthorizeAsync(ctx, Permission.ManageDefinition, ct);
        // definition.Validate();
        // Register with Elsa engine via adapter
        _logger.LogInformation(
        "GetWorkflowDefinitionsAsync",
        "[Service] Calling Workflow Engine");

        var result = await _engineAdapter.GetWorkflowDefinitionsAsync(request, ctx, ct);

        _logger.LogInformation(
            "GetWorkflowDefinitionsAsync",
            "[Service] Called Workflow Engine Successfully");

        return result;
        //definition.Publish(ctx.UserId);


        //return await _repo.SaveAsync(definition, ct);

    }
    public async Task<WorkflowDefinitionDetailPagedResponse> GetWorkflowDefinitionsDetailAsync(
    WorkflowDefinitionQueryRequest request,
    ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        
        await _auth.AuthorizeAsync(ctx, Permission.ManageDefinition, cancellationToken);
        
        // definition.Validate();
        // Register with Elsa engine via adapter
        _logger.LogInformation(
        "GetWorkflowDefinitionsDetailAsync",
        "[Service] Calling Workflow Engine");

        var result = await _engineAdapter.GetWorkflowDefinitionsDetailAsync(request, ctx, cancellationToken);

        _logger.LogInformation(
            "GetWorkflowDefinitionsDetailAsync",
            "[Service] Called Workflow Engine Successfully");

        return result;
    }
    public async Task<ActivityExecutionBriefPagedResponse> GetWorkflowInstanceActivityAsync(
    WorkflowDefinitionQueryRequest request,
    string ExecutionId,
    ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        await _auth.AuthorizeAsync(ctx, Permission.ManageDefinition, cancellationToken);
        // definition.Validate();
        _logger.LogInformation(
        "GetWorkflowInstanceActivityAsync",
        "[Service] Calling Workflow Engine");

        var result = await _engineAdapter.GetActivityExecutionsAsync(request, ExecutionId, ctx, cancellationToken);

        _logger.LogInformation(
            "GetWorkflowInstanceActivityAsync",
            "[Service] Called Workflow Engine Successfully");
        // Register with Elsa engine via adapter

        return result;
    }
    public async Task<ActivityExecutionDetailPagedResponse> GetWorkflowInstanceActivityDetailAsync(
    WorkflowDefinitionQueryRequest request,
    string ExecutionId,
    ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        await _auth.AuthorizeAsync(ctx, Permission.ManageDefinition, cancellationToken);
        // definition.Validate();
        _logger.LogInformation(
        "GetWorkflowInstanceActivityDetailAsync",
        "[Service] Calling Workflow Engine");

        var result = await _engineAdapter.GetActivityExecutionsFullAsync(request, ExecutionId, ctx, cancellationToken);

        _logger.LogInformation(
            "GetWorkflowInstanceActivityDetailAsync",
            "[Service] Called Workflow Engine Successfully");
        // Register with Elsa engine via adapter

        return result;
    }
    public async Task<LastExecutedWorkflowActivityResponse> GetLastExecutedWorkflowActivityAsync(
    WorkflowDefinitionQueryRequest request,
    string DefinitionId,
    ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        await _auth.AuthorizeAsync(ctx, Permission.ManageDefinition, cancellationToken);
        // definition.Validate();
        _logger.LogInformation(
        "GetLastExecutedWorkflowActivityAsync",
        "[Service] Calling Workflow Engine");

        var result = await _engineAdapter.GetLastExecutedWorkflowActivityAsync(request, DefinitionId, ctx, cancellationToken);

        _logger.LogInformation(
            "GetLastExecutedWorkflowActivityAsync",
            "[Service] Called Workflow Engine Successfully");
        // Register with Elsa engine via adapter

        return result;
    }
    public async Task<LastExecutedWorkflowActivityDetailResponse> GetLastExecutedWorkflowActivityDetailAsync(
    WorkflowDefinitionQueryRequest request,
    string DefinitionId,
    ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        await _auth.AuthorizeAsync(ctx, Permission.ManageDefinition, cancellationToken);
        // definition.Validate();
        _logger.LogInformation(
        "GetLastExecutedWorkflowActivityDetailAsync",
        "[Service] Calling Workflow Engine");

        var result = await _engineAdapter.GetLastExecutedWorkflowActivityDetailAsync(request, DefinitionId, ctx, cancellationToken);

        _logger.LogInformation(
            "GetLastExecutedWorkflowActivityDetailAsync",
            "[Service] Called Workflow Engine Successfully");
        // Register with Elsa engine via adapter

        return result;
    }
}
