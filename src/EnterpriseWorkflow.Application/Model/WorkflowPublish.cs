using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Model
{

    public record WorkflowPublishRequest(
     string WorkflowCode,
     string? WorkflowDescription,
     int OrgId,
     long DomainId,
     string VersionNumber,
     List<WorkflowNodeDto> Nodes,
     List<WorkflowTransitionDto> Transitions
 );

    public record WorkflowNodeDto(
        string NodeTableType,
        long NodeId,       
        string EngineActivityReference,
        string? ConfigJson,
        long SequenceNumber//,
       // string? Type
    );

    public record WorkflowTransitionDto(
        string FromNodeType,
        long FromNodeId,
        string ToNodeType,
        long ToNodeId,
        string TransitionPathType,
        int SequenceNumber
    );

    public record WorkflowPublishResult(
        bool Success,
        string? ErrorMessage = null
    );
}
