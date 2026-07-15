using EnterpriseWorkflow.Domain.Entities.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Storage.Interfaces.Repositories
{
    public interface IWorkflowCreationRepository
    {
        Task<List<List<Dictionary<string, object>>>> GetSlaAsync(GetSlaRequest request);
        Task<List<List<Dictionary<string, object>>>> GetAuthenticationProviderAsync(GetAuthenticationProviderRequest request);
        Task<List<List<Dictionary<string, object>>>> GetAuthenticationPolicyAsync(GetAuthenticationPolicyRequest request);
        Task<List<List<Dictionary<string, object>>>> GetOrgLevelDefaultPolicyAsync(GetOrgLevelDefaultPolicyRequest request);
        Task<List<List<Dictionary<string, object>>>> GetAuthorizationPolicyAsync(GetAuthorizationPolicyRequest request);
        Task<List<List<Dictionary<string, object>>>> GetStageAsync(GetStageRequest request);
        Task<List<List<Dictionary<string, object>>>> GetStepAsync(GetStepRequest request);
        Task<List<List<Dictionary<string, object>>>> GetTaskAsync(GetTaskRequest request);
        Task<List<List<Dictionary<string, object>>>> GetActionAsync(GetActionRequest request);
        Task<List<List<Dictionary<string, object>>>> GetEntityAsync(GetEntityRequest request);
        Task<List<List<Dictionary<string, object>>>> GetEntityAssociationAsync(GetEntityAssociationRequest request);
        Task<List<List<Dictionary<string, object>>>> GetRuleAsync(GetRuleRequest request);
        Task<List<List<Dictionary<string, object>>>> GetRuleSetAsync(GetRuleSetRequest request);
        Task<List<List<Dictionary<string, object>>>> GetDomainAsync(GetDomainRequest request);
        Task<CommonDBResponse> SaveRuleSetAsync(SaveRuleSetRequest request, string correlationId);

        Task<CommonDBResponse> SaveAuthenticationProviderAsync(SaveAuthenticationProviderRequest request, string correlationId);

        Task<CommonDBResponse> SaveAuthenticationPolicyAsync(SaveAuthenticationPolicyRequest request, string correlationId);

        Task<CommonDBResponse> SaveOrgLevelDefaultPolicyAsync(SaveOrgLevelDefaultPolicyRequest request, string correlationId);

        Task<CommonDBResponse> SaveDomainAsync(SaveDomainRequest request, string correlationId);

        Task<CommonDBResponse> SaveSlaAsync(SaveSlaRequest request, string correlationId);

        Task<CommonDBResponse> SaveStageAsync(SaveStageRequest request, string correlationId);

        Task<CommonDBResponse> SaveStepAsync(SaveStepRequest request, string correlationId);

        Task<CommonDBResponse> SaveTaskAsync(SaveTaskRequest request, string correlationId);

        Task<CommonDBResponse> SaveActionAsync(SaveActionRequest request, string correlationId);

        Task<CommonDBResponse> SaveEntityAsync(SaveEntityRequest request, string correlationId);

        Task<CommonDBResponse> SaveEntityAssociationAsync(SaveEntityAssociationRequest request, string correlationId);

        Task<CommonDBResponse> SaveRuleAsync(SaveRuleRequest request, string correlationId);

        Task<CommonDBResponse> SaveAuthorizationPolicyAsync(SaveAuthorizationPolicyRequest request, string correlationId);

        Task<CommonDBResponse> SaveEventAsync(SaveEventRequest request, string correlationId);
        Task<CommonDBResponse> SaveExecutionPolicyAsync(SaveExecutionPolicyRequest request, string correlationId);
        Task<CommonDBResponse> SaveWorkflowAsync(SaveWorkflowRequest request, string correlationId);
        Task<CommonDBResponse> SaveWorkflowTransitionsAsync(SaveWorkflowTransitionsRequest request, string correlationId);
    }
}
