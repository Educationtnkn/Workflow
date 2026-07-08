using EnterpriseWorkflow.Application.Ports.Inbound.Interface;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.Enums;
using EnterpriseWorkflow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Services;
public class WorkflowDefinitionService : IWorkflowDefinition
{
    private readonly IWorkflowDefinitionRepository _repo;
    private readonly IWorkflowEngineAdapter _engineAdapter;
    private readonly IAuthorizationHandler _auth;

    public WorkflowDefinitionService(IWorkflowDefinitionRepository repo, IWorkflowEngineAdapter engineAdapter, IAuthorizationHandler auth)
    {
        _repo = repo;
        _engineAdapter = engineAdapter;
        _auth = auth;
    }

    public async Task<string> CreateDefinitionAsync(Workflowdefinition definition, ExecutionModel ctx, CancellationToken ct = default)
    {
        await _auth.AuthorizeAsync(ctx, Permission.ManageDefinition, ct);
       // definition.Validate();
        // Register with Elsa engine via adapter

        return await _engineAdapter.RegisterDefinitionAsync(definition,ctx, ct);
        //definition.Publish(ctx.UserId);
        

        //return await _repo.SaveAsync(definition, ct);

    }
    public async Task<string> CreateDefinitionAsync(
                string json,
                CancellationToken ct = default)
    {
        //await _auth.AuthorizeAsync(ctx,Permission.ManageDefinition, ct);
        // definition.Validate();
        // Register with Elsa engine via adapter

        return await _engineAdapter.CreateDefinitionAsync( json, ct);
    }


    public async Task<string> LoadAndCreateDefinitionAsync(string fileName, CancellationToken ct = default)
    {
        return await _engineAdapter.LoadAndCreateAsync( fileName, ct);
    }
    public async Task PublishDefinitionAsync(string definitionId, ExecutionModel ctx, CancellationToken ct = default)
    {
        //await _auth.AuthorizeAsync(ctx, Permission.ManageDefinition, ct);
        
        //var definition = await _repo.GetByIdAsync(definitionId, ct);
        //if (definition == null)
        //    throw new KeyNotFoundException($"Definition {definitionId} not found");

        //// Register with Elsa engine via adapter
        //await _engineAdapter.RegisterDefinitionAsync(definition,ctx, ct);

     
       // definition.Publish(ctx.UserId);
        //await _repo.UpdateAsync(definition, ct);
    }


    public async Task<Workflowdefinition?> GetDefinitionAsync(string id, ExecutionModel ctx, CancellationToken ct = default)
    {
        return null;
    }

}

