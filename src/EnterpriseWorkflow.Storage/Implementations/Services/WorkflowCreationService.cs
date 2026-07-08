using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Storage.Interfaces.Repositories;
using EnterpriseWorkflow.Storage.Interfaces.Services;
using EnterpriseWorkflow.Utilities.Observability;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Traces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Storage.Implementations.Services
{
    public class WorkflowCreationService : IWorkflowCreationService
    {
        private readonly IWorkflowCreationRepository _workflowPort;
        private readonly ILoggingService _logger;
        private readonly INDTracerService _tracer;

        public WorkflowCreationService(
            IWorkflowCreationRepository workflowPort,
            ILoggingService logger,
            INDTracerService tracer)
        {
            _workflowPort = workflowPort;
            _logger = logger;
            _tracer = tracer;
        }
        public async Task<List<List<Dictionary<string, object>>>> GetSlaAsync(GetSlaRequest request)
        {
            const string svc = "GetSla";
            var filters = Filters("SlaId", request.SlaId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetSla Service Started", filterIds: filters);
            _tracer.AddEvent("get_sla_service_started");
            try
            {
                var result = await _workflowPort.GetSlaAsync(request);
                _logger.LogInformation(svc, $"GetSla Service Completed {result.Count}", filterIds: filters);
                return result;
            }
            catch (Exception ex) { LogAndRethrow(svc, ex, filters); throw; }
        }

        public async Task<List<List<Dictionary<string, object>>>> GetAuthenticationProviderAsync(GetAuthenticationProviderRequest request)
        {
            const string svc = "GetAuthenticationProvider";
            var filters = Filters("AuthenticationProviderId", request.AuthenticationProviderId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetAuthenticationProvider Service Started", filterIds: filters);
            _tracer.AddEvent("get_authentication_provider_service_started");
            try
            {
                var result = await _workflowPort.GetAuthenticationProviderAsync(request);
                _logger.LogInformation(svc, $"GetAuthenticationProvider Service Completed  {result.Count}", filterIds: filters);
                return result;
            }
            catch (Exception ex) { LogAndRethrow(svc, ex, filters); throw; }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetAuthenticationPolicyAsync(GetAuthenticationPolicyRequest request)
        {
            const string svc = "GetAuthenticationPolicy";
            var filters = Filters("AuthenticationPolicyId", request.AuthenticationPolicyId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetAuthenticationPolicy Service Started", filterIds: filters);
            _tracer.AddEvent("get_authentication_policy_service_started");
            try
            {
                var result = await _workflowPort.GetAuthenticationPolicyAsync(request);
                _logger.LogInformation(svc, $"GetAuthenticationPolicy Service Completed {result.Count}", filterIds: filters);
                return result;
            }
            catch (Exception ex) { LogAndRethrow(svc, ex, filters); throw; }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetOrgLevelDefaultPolicyAsync(GetOrgLevelDefaultPolicyRequest request)
        {
            const string svc = "GetOrgLevelDefaultPolicy";
            var filters = Filters("OrgLevelDefaultPolicyId", request.OrgLevelDefaultPolicyId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetOrgLevelDefaultPolicy Service Started", filterIds: filters);
            _tracer.AddEvent("get_org_level_default_policy_service_started");
            try
            {
                var result = await _workflowPort.GetOrgLevelDefaultPolicyAsync(request);
                _logger.LogInformation(svc, $"GetOrgLevelDefaultPolicy Service Completed {result.Count}", filterIds: filters);
                return result;
            }
            catch (Exception ex) { LogAndRethrow(svc, ex, filters); throw; }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetAuthorizationPolicyAsync(GetAuthorizationPolicyRequest request)
        {
            const string svc = "GetAuthorizationPolicy";
            var filters = Filters("AuthorizationPolicyId", request.AuthorizationPolicyId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetAuthorizationPolicy Service Started", filterIds: filters);
            try { 
                var result = await _workflowPort.GetAuthorizationPolicyAsync(request); 
                _logger.LogInformation(svc, $"GetAuthorizationPolicy completed - Records: {result.Count}", filterIds: filters); 
                return result; 
            }
            catch (Exception ex) { 
                LogAndRethrow(svc, ex, filters); 
                throw; 
            }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetStageAsync(GetStageRequest request)
        {
            const string svc = "GetStage";
            var filters = Filters("StageId", request.StageId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetStage Service Started", filterIds: filters);
            try { 
                var response = await _workflowPort.GetStageAsync(request); 
               _logger.LogInformation(svc, $"GetStage completed {response.Count}", filterIds: filters); 
                return response; 
            }
            catch (Exception ex) { 
                LogAndRethrow(svc, ex, filters); 
                throw;
            }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetStepAsync(GetStepRequest request)
        {
            const string svc = "GetStep";
            var filters = Filters("StepId", request.StepId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetStep Service Started", filterIds: filters);
            try { 
                var response = await _workflowPort.GetStepAsync(request);
                _logger.LogInformation(svc, $"GetStep completed  {response.Count}", filterIds: filters); 
                return response; 
            }
            catch (Exception ex) {
                LogAndRethrow(svc, ex, filters); 
                throw; 
            }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetTaskAsync(GetTaskRequest request)
        {
            const string svc = "GetTask";
            var filters = Filters("TaskId", request.TaskId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetTask Service Started", filterIds: filters);
            try { 
                var response = await _workflowPort.GetTaskAsync(request); 
                _logger.LogInformation(svc, $"GetTask completed  {response.Count}", filterIds: filters); 
                return response; 
            }
            catch (Exception ex) { 
                LogAndRethrow(svc, ex, filters); 
                throw; 
            }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetActionAsync(GetActionRequest request)
        {
            const string svc = "GetAction";
            var filters = Filters("ActionId", request.ActionId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetAction Service Started", filterIds: filters);
            try { 
                var response = await _workflowPort.GetActionAsync(request); 
                _logger.LogInformation(svc, $"GetAction completed {response.Count}", filterIds: filters); 
                return response; 
            }
            catch (Exception ex) {
                LogAndRethrow(svc, ex, filters); 
                throw; 
            }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetEntityAsync(GetEntityRequest request)
        {
            const string svc = "GetEntity";
            var filters = Filters("EntityId", request.EntityId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetEntity Service Started", filterIds: filters);
            try { 
                var response = await _workflowPort.GetEntityAsync(request); 
                _logger.LogInformation(svc, $"GetEntity completed {response.Count}", filterIds: filters); 
                return response; 
            }
            catch (Exception ex) { 
                LogAndRethrow(svc, ex, filters); 
                throw; 
            }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetEntityAssociationAsync(GetEntityAssociationRequest request)
        {
            const string svc = "GetEntityAssociation";
            var filters = Filters("EntityAssociationId", request.EntityAssociationId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetEntityAssociation Service Started", filterIds: filters);
            try { 
                var response = await _workflowPort.GetEntityAssociationAsync(request); 
                _logger.LogInformation(svc, $"GetEntityAssociation completed  {response.Count}", filterIds: filters); 
                return response; 
            }
            catch (Exception ex) { 
                LogAndRethrow(svc, ex, filters); 
                throw; 
            }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetRuleAsync(GetRuleRequest request)
        {
            const string svc = "GetRule";
            var filters = Filters("RuleId", request.RuleId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetRule Service Started", filterIds: filters);
            try { 
                var response = await _workflowPort.GetRuleAsync(request); 
                _logger.LogInformation(svc, $"GetRule completed {response.Count}", filterIds: filters); 
                return response; 
            }
            catch (Exception ex) {
                LogAndRethrow(svc, ex, filters); 
                throw;
            }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetRuleSetAsync(GetRuleSetRequest request)
        {
            const string svc = "GetRuleSet";
            var filters = Filters("RuleSetId", request.RuleSetId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetRuleSet Service Started", filterIds: filters);
            _tracer.AddEvent("get_rule_set_service_started");
            try
            {
                var result = await _workflowPort.GetRuleSetAsync(request);
                _logger.LogInformation(svc, $"GetRuleSet Service Completed {result.Count}", filterIds: filters);
                return result;
            }
            catch (Exception ex) { 
                LogAndRethrow(svc, ex, filters); 
                throw; 
            }
        }
        public async Task<List<List<Dictionary<string, object>>>> GetDomainAsync(GetDomainRequest request)
        {
            const string svc = "GetDomain";
            var filters = Filters("DomainId", request.DomainId, "OrgId", request.OrgId);
            _logger.LogInformation(svc, "GetDomain Service Started", filterIds: filters);
            _tracer.AddEvent("get_domain_service_started");
            try
            {
                var result = await _workflowPort.GetDomainAsync(request);
                _logger.LogInformation(svc, $"GetDomain Service Completed {result.Count}", filterIds: filters);
                return result;
            }
            catch (Exception ex) { LogAndRethrow(svc, ex, filters); throw; }
        }
        //Helper

        private static Dictionary<string, object> Filters(string k1, object? v1, string? k2 = null, object? v2 = null)
        {
            var d = new Dictionary<string, object> { [k1] = v1?.ToString() ?? "NULL" };
            if (k2 != null) d[k2] = v2?.ToString() ?? "NULL";
            return d;
        }
        private void LogAndRethrow(string svc, Exception ex, Dictionary<string, object> filters)
        {
            _tracer.AddEvent($"{svc.ToLower()}_service_failed", new() { ["error"] = ex.Message });
            _logger.LogError(svc, $"{svc} Service Error",
                errorDetails: ex.Message, filterIds: filters, exception: ex);
        }


        /// <summary>
        /// Validates and forwards a RuleSet save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveRuleSetAsync(SaveRuleSetRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveRuleSetAsync";
            var filterIds = new Dictionary<string, object> { { "RuleSetId", request.RuleSetId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveRuleSetAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a AuthenticationProvider save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveAuthenticationProviderAsync(SaveAuthenticationProviderRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveAuthenticationProviderAsync";
            var filterIds = new Dictionary<string, object> { { "AuthenticationProviderId", request.AuthenticationProviderId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveAuthenticationProviderAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a AuthenticationPolicy save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveAuthenticationPolicyAsync(SaveAuthenticationPolicyRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveAuthenticationPolicyAsync";
            var filterIds = new Dictionary<string, object> { { "AuthenticationPolicyId", request.AuthenticationPolicyId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveAuthenticationPolicyAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a OrgLevelDefaultPolicy save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveOrgLevelDefaultPolicyAsync(SaveOrgLevelDefaultPolicyRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveOrgLevelDefaultPolicyAsync";
            var filterIds = new Dictionary<string, object> { { "OrgLevelDefaultPolicyId", request.OrgLevelDefaultPolicyId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveOrgLevelDefaultPolicyAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a Domain save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveDomainAsync(SaveDomainRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveDomainAsync";
            var filterIds = new Dictionary<string, object> { { "DomainId", request.DomainId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveDomainAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a Sla save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveSlaAsync(SaveSlaRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveSlaAsync";
            var filterIds = new Dictionary<string, object> { { "SlaId", request.SlaId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveSlaAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a Stage save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveStageAsync(SaveStageRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveStageAsync";
            var filterIds = new Dictionary<string, object> { { "StageId", request.StageId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveStageAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a Step save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveStepAsync(SaveStepRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveStepAsync";
            var filterIds = new Dictionary<string, object> { { "StepId", request.StepId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveStepAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a TaskDefinition save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveTaskAsync(SaveTaskRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveTaskAsync";
            var filterIds = new Dictionary<string, object> { { "TaskId", request.TaskId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveTaskAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a ActionDefinition save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveActionAsync(SaveActionRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveActionAsync";
            var filterIds = new Dictionary<string, object> { { "ActionDefinitionId", request.ActionDefinitionId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveActionAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a EntityDefinition save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveEntityAsync(SaveEntityRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveEntityAsync";
            var filterIds = new Dictionary<string, object> { { "EntityDefinitionId", request.EntityDefinitionId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveEntityAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a EntityAssociation save request to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveEntityAssociationAsync(SaveEntityAssociationRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveEntityAssociationAsync";
            var filterIds = new Dictionary<string, object> { { "WorkflowEntityAssociationId", request.WorkflowEntityAssociationId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveEntityAssociationAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards a Rule save request (with its Rule_CONFIG list) to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveRuleAsync(SaveRuleRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveRuleAsync";
            var filterIds = new Dictionary<string, object> { { "RuleId", request.RuleId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveRuleAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Validates and forwards an Authorization Policy save request (with its rule list) to the repository layer.
        /// </summary>
        public async Task<CommonDBResponse> SaveAuthorizationPolicyAsync(SaveAuthorizationPolicyRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveAuthorizationPolicyAsync";
            var filterIds = new Dictionary<string, object> { { "AuthorizationPolicyId", request.AuthorizationPolicyId ?? 0L } };

            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);

            try
            {
                var result = await _workflowPort.SaveAuthorizationPolicyAsync(request, correlationId);

                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }


        /// <summary>
        /// Creates or updates a Workflow Event definition, including its
        /// Event Action table-valued list.
        /// </summary>
        public async Task<CommonDBResponse> SaveEventAsync(SaveEventRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveEventAsync";
            var filterIds = new Dictionary<string, object> { { "EventId", request.EventId ?? 0L } };
            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);
            try
            {
                var result = await _workflowPort.SaveEventAsync(request, correlationId);
                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Execution Policy, including its
        /// Execution Policy Config table-valued list.
        /// </summary>
        public async Task<CommonDBResponse> SaveExecutionPolicyAsync(SaveExecutionPolicyRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveExecutionPolicyAsync";
            var filterIds = new Dictionary<string, object> { { "ExecutionPolicyId", request.ExecutionPolicyId ?? 0L } };
            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);
            try
            {
                var result = await _workflowPort.SaveExecutionPolicyAsync(request, correlationId);
                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Creates or updates a Workflow definition together with its embedded
        /// Workflow Version.
        /// </summary>
        public async Task<CommonDBResponse> SaveWorkflowAsync(SaveWorkflowRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveWorkflowAsync";
            var filterIds = new Dictionary<string, object> { { "WorkflowId", request.WorkflowId ?? 0L } };
            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);
            try
            {
                var result = await _workflowPort.SaveWorkflowAsync(request, correlationId);
                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Replaces the full set of Workflow Transitions for a given Workflow Version.
        /// </summary>
        public async Task<CommonDBResponse> SaveWorkflowTransitionsAsync(SaveWorkflowTransitionsRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigService.SaveWorkflowTransitionsAsync";
            var filterIds = new Dictionary<string, object> { { "WorkflowVersionId", request.WorkflowVersionId } };
            _logger.LogInformation(serviceName, "Service Started", correlationId: correlationId, filterIds: filterIds);
            try
            {
                var result = await _workflowPort.SaveWorkflowTransitionsAsync(request, correlationId);
                _logger.LogInformation(serviceName, "Service Ended", correlationId: correlationId, filterIds: filterIds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Service Failed",
                    errorCode: "SERVICE_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }
    }
}
