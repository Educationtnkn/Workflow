// csharp
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
//using Microsoft.EntityFrameworkCore;

namespace EnterpriseWorkflow.Application.Ports.Outbound.Implemenation;

public class WorkFlowEngine 
    //: IWorkflowEngineAdapter
{
    //private readonly EnterpriseWorkflowDbContext _db;
    //public WorkFlowEngine(EnterpriseWorkflowDbContext db) => _db = db;

    //public string EngineType => "EntityFramework";

    //public async Task RegisterDefinitionAsync(Workflowdefinition definition, CancellationToken ct = default)
    //{
    //    if (definition is null) throw new ArgumentNullException(nameof(definition));

    //    // Attach or Add depending on whether Id is set; simple Add works for new definitions.
    //    _db.WorkflowDefinitions.Add(definition);
    //    await _db.SaveChangesAsync(ct);
    //}

    public record WorkflowRow(long WorkflowId, string WorkflowCode, string WorkflowName, string? WorkflowDescription);
    public record VersionRow(long VersionId, long WorkflowId, int OrgId, long DomainId, string VersionNumber);
    public record NodeRow(string NodeTableType, long NodeId, string EngineActivityReference, string? ConfigJson, long SequenceNumber);
    public record TransitionRow(string FromNodeType, long FromNodeId, string ToNodeType, long ToNodeId, string TransitionPathType, int SequenceNumber);
}