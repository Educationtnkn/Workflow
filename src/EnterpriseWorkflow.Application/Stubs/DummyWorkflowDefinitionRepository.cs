using System.Collections.Concurrent;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;

namespace EnterpriseWorkflow.Application.Stubs;

//public class DummyWorkflowDefinitionRepository : IWorkflowDefinitionRepository
//{
//    private readonly ConcurrentDictionary<string, Workflowdefinition> _store = new();

//    public Task<Workflowdefinition?> GetByIdAsync(string id, string tenantId, string domainId, CancellationToken ct = default)
//    {
//        _store.TryGetValue(id, out var def);
//        // Optional tenant/domain filter
//        //if (def = null && (def.TenantId != tenantId || def.DomainId != domainId))
//       // if (def == null)
//        return Task.FromResult<Workflowdefinition?>(null);
        
//        //return Task.FromResult(def);
//    }

//    public Task<string> SaveAsync(Workflowdefinition definition, CancellationToken ct = default)
//    {
//        //if (string.IsNullOrWhiteSpace(definition.Id))
//            //definition.Id = Guid.NewGuid().ToString();
//        _store[definition.Id] = definition;
//        return Task.FromResult(definition.Id);
//    }

//    public Task UpdateAsync(Workflowdefinition definition, CancellationToken ct = default)
//    {
//        if (!_store.ContainsKey(definition.Id))
//            throw new KeyNotFoundException($"Definition {definition.Id} not found.");
//        _store[definition.Id] = definition;
//        return Task.CompletedTask;
//    }
//}