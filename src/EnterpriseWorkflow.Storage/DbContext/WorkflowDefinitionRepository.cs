using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Storage.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Storage.DbContext
{
    public  class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
    {
        //private readonly WorkflowDbContext _db;
        //public WorkflowDefinitionRepository(WorkflowDbContext db) => _db = db;

        //public async Task<Workflowdefinition?> GetByIdAsync(string id, CancellationToken ct)
        //    => await _db.WorkflowDefinitions.Include(d => d.Steps).ThenInclude(s => s.Tasks).ThenInclude(t => t.Actions)
        //        .FirstOrDefaultAsync(d => d.DefinitionId == id, ct);

        //public async Task<Workflowdefinition?> GetByDefinitionIdAndVersionAsync(string definitionId, int version, CancellationToken ct)
        //    => await _db.WorkflowDefinitions.FirstOrDefaultAsync(v => v.DefinitionId == definitionId && v.VersionNo     == version, ct);

        //public async Task<Workflowdefinition?> GetPublishedAsync(string definitionId, CancellationToken ct)
        //    => await _db.WorkflowDefinitions.FirstOrDefaultAsync(v => v.DefinitionId == definitionId && v.IsPublished == true, ct);

        //public async Task<string> SaveAsync(Workflowdefinition definition, CancellationToken ct)
        //{
        //    _db.WorkflowDefinitions.Add(definition);
        //    await _db.SaveChangesAsync(ct);
        //    return definition.DefinitionId;
        //}

        //public async Task UpdateAsync(Workflowdefinition definition, CancellationToken ct)
        //{
        //    _db.Entry(definition).State = EntityState.Modified;
        //    await _db.SaveChangesAsync(ct);
        //}

        //public async Task<bool> ExistsAsync(string definitionId, CancellationToken ct)
        //    => await _db.WorkflowDefinitions.AnyAsync(d => d.DefinitionId == definitionId, ct);
    }
}
