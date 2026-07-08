using EnterpriseWorkflow.Application.Ports.Inbound;
using EnterpriseWorkflow.Application.Ports.Inbound.Interface;
using EnterpriseWorkflow.Application.Services;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.ValueObjects;
using EnterpriseWorkflow.Storage.Interfaces.Services;
using EnterpriseWorkflow.Utilities.Observability;
using Microsoft.AspNetCore.Mvc;
using ND.ExceptionFramework.Core.Application.DTO;
using ND.ExceptionFramework.Core.Application.Handlers.Interface;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Traces;
using System.Diagnostics;

namespace EnterpriseWorkflow.Api.Controller

{
    [ApiController]
    [Route("api/v1/workFlow/")]
    public class WorkflowCreationController: ControllerBase
    {
        private readonly IWorkflowCreationService _workflowService;
        private readonly ILoggingService _logger;
        private readonly INDTracerService _tracer;
        private readonly string CorrelationId= Guid.NewGuid().ToString();
        private readonly IExceptionHandler _exceptionHandler;
        public WorkflowCreationController(
            IWorkflowCreationService workflowService,
            INDTracerService tracer,
            ILoggingService logger,
            IExceptionHandler exceptionHandler)
        {
            _workflowService = workflowService;
            _tracer = tracer;
            _logger = logger;
            _exceptionHandler = exceptionHandler;   
        }
        [HttpPost("getSla")]
        public async Task<IActionResult> GetSla([FromBody] GetSlaRequest request)
        {
            using var span = _tracer.StartSpan("get_sla", new()
            {
                ["user.id"] = request.UserId,
                ["sla.id"] = request.SlaId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetSla";
            var filterIds = new Dictionary<string, object>
            {
                ["SlaId"] = request.SlaId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetSla API Started", filterIds: filterIds);
                _tracer.AddEvent("get_sla_request_started");


                var result = await _workflowService.GetSlaAsync(request);

                _tracer.AddEvent("get_sla_completed_successfully");
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetSla API Ended", filterIds: filterIds);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.AddEvent("get_sla_failed", new() { ["error"] = ex.Message });
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetSla API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getOrgLevelDefaultPolicy")]
        public async Task<IActionResult> GetOrgLevelDefaultPolicy([FromBody] GetOrgLevelDefaultPolicyRequest request)
        {
            using var span = _tracer.StartSpan("get_org_level_default_policy", new()
            {
                ["user.id"] = request.UserId,
                ["org_level_default_policy.id"] = request.OrgLevelDefaultPolicyId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetOrgLevelDefaultPolicy";
            var filterIds = new Dictionary<string, object>
            {
                ["OrgLevelDefaultPolicyId"] = request.OrgLevelDefaultPolicyId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetOrgLevelDefaultPolicy API Started", filterIds: filterIds);
                _tracer.AddEvent("get_org_level_default_policy_request_started");


                var result = await _workflowService.GetOrgLevelDefaultPolicyAsync(request);

                _tracer.AddEvent("get_org_level_default_policy_completed_successfully");
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetOrgLevelDefaultPolicy API Ended", filterIds: filterIds);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.AddEvent("get_org_level_default_policy_failed", new() { ["error"] = ex.Message });
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetOrgLevelDefaultPolicy API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getAuthenticationProvider")]
        public async Task<IActionResult> GetAuthenticationProvider([FromBody] GetAuthenticationProviderRequest request)
        {
            using var span = _tracer.StartSpan("get_authentication_provider", new()
            {
                ["user.id"] = request.UserId,
                ["authentication_provider.id"] = request.AuthenticationProviderId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetAuthenticationProvider";
            var filterIds = new Dictionary<string, object>
            {
                ["AuthenticationProviderId"] = request.AuthenticationProviderId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetAuthenticationProvider API Started", filterIds: filterIds);
                _tracer.AddEvent("get_authentication_provider_request_started");


                var result = await _workflowService.GetAuthenticationProviderAsync(request);

                _tracer.AddEvent("get_authentication_provider_completed_successfully");
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetAuthenticationProvider API Ended", filterIds: filterIds);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.AddEvent("get_authentication_provider_failed", new() { ["error"] = ex.Message });
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetAuthenticationProvider API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getAuthenticationPolicy")]
        public async Task<IActionResult> GetAuthenticationPolicy([FromBody] GetAuthenticationPolicyRequest request)
        {
            using var span = _tracer.StartSpan("get_authentication_policy", new()
            {
                ["user.id"] = request.UserId,
                ["authentication_policy.id"] = request.AuthenticationPolicyId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetAuthenticationPolicy";
            var filterIds = new Dictionary<string, object>
            {
                ["AuthenticationPolicyId"] = request.AuthenticationPolicyId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetAuthenticationPolicy API Started", filterIds: filterIds);
                _tracer.AddEvent("get_authentication_policy_request_started");


                var result = await _workflowService.GetAuthenticationPolicyAsync(request);

                _tracer.AddEvent("get_authentication_policy_completed_successfully");
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetAuthenticationPolicy API Ended", filterIds: filterIds);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.AddEvent("get_authentication_policy_failed", new() { ["error"] = ex.Message });
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetAuthenticationPolicy API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getAuthorizationPolicy")]
        public async Task<IActionResult> GetAuthorizationPolicy([FromBody] GetAuthorizationPolicyRequest request)
        {
            using var span = _tracer.StartSpan("get_authorization_policy", new()
            {
                ["user.id"] = request.UserId,
                ["authorization_policy.id"] = request.AuthorizationPolicyId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetAuthorizationPolicy";
            var filterIds = new Dictionary<string, object>
            {
                ["AuthorizationPolicyId"] = request.AuthorizationPolicyId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetAuthorizationPolicy API Started", filterIds: filterIds);
                var result = await _workflowService.GetAuthorizationPolicyAsync(request);
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetAuthorizationPolicy API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetAuthorizationPolicy API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }

        [HttpPost("getStage")]
        public async Task<IActionResult> GetStage([FromBody] GetStageRequest request)
        {
            using var span = _tracer.StartSpan("get_stage", new()
            {
                ["user.id"] = request.UserId,
                ["stage.id"] = request.StageId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetStage";
            var filterIds = new Dictionary<string, object>
            {
                ["StageId"] = request.StageId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetStage API Started", filterIds: filterIds);
                var result = await _workflowService.GetStageAsync(request);
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetStage API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetStage API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getStep")]
        public async Task<IActionResult> GetStep([FromBody] GetStepRequest request)
        {
            using var span = _tracer.StartSpan("get_step", new()
            {
                ["user.id"] = request.UserId,
                ["step.id"] = request.StepId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetStep";
            var filterIds = new Dictionary<string, object>
            {
                ["StepId"] = request.StepId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetStep API Started", filterIds: filterIds);
                var result = await _workflowService.GetStepAsync(request);
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetStep API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetStep API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getTask")]
        public async Task<IActionResult> GetTask([FromBody] GetTaskRequest request)
        {
            using var span = _tracer.StartSpan("get_task", new()
            {
                ["user.id"] = request.UserId,
                ["task.id"] = request.TaskId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetTask";
            var filterIds = new Dictionary<string, object>
            {
                ["TaskId"] = request.TaskId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetTask API Started", filterIds: filterIds);
                var result = await _workflowService.GetTaskAsync(request);
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetTask API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetTask API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getAction")]
        public async Task<IActionResult> GetAction([FromBody] GetActionRequest request)
        {
            using var span = _tracer.StartSpan("get_action", new()
            {
                ["user.id"] = request.UserId,
                ["action.id"] = request.ActionId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetAction";
            var filterIds = new Dictionary<string, object>
            {
                ["ActionId"] = request.ActionId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetAction API Started", filterIds: filterIds);
                var result = await _workflowService.GetActionAsync(request);
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetAction API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetAction API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getEntity")]
        public async Task<IActionResult> GetEntity([FromBody] GetEntityRequest request)
        {
            using var span = _tracer.StartSpan("get_entity", new()
            {
                ["user.id"] = request.UserId,
                ["entity.id"] = request.EntityId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetEntity";
            var filterIds = new Dictionary<string, object>
            {
                ["EntityId"] = request.EntityId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetEntity API Started", filterIds: filterIds);
                var result = await _workflowService.GetEntityAsync(request);
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetEntity API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetEntity API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getEntityAssociation")]
        public async Task<IActionResult> GetEntityAssociation([FromBody] GetEntityAssociationRequest request)
        {
            using var span = _tracer.StartSpan("get_entity_association", new()
            {
                ["user.id"] = request.UserId,
                ["entity_association.id"] = request.EntityAssociationId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetEntityAssociation";
            var filterIds = new Dictionary<string, object>
            {
                ["EntityAssociationId"] = request.EntityAssociationId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetEntityAssociation API Started", filterIds: filterIds);
                var result = await _workflowService.GetEntityAssociationAsync(request);
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetEntityAssociation API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetEntityAssociation API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getRule")]
        public async Task<IActionResult> GetRule([FromBody] GetRuleRequest request)
        {
            using var span = _tracer.StartSpan("get_rule", new()
            {
                ["user.id"] = request.UserId,
                ["rule.id"] = request.RuleId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetRule";
            var filterIds = new Dictionary<string, object>
            {
                ["RuleId"] = request.RuleId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetRule API Started", filterIds: filterIds);
                var result = await _workflowService.GetRuleAsync(request);
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetRule API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetRule API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getRuleSet")]
        public async Task<IActionResult> GetRuleSet([FromBody] GetRuleSetRequest request)
        {
            using var span = _tracer.StartSpan("get_rule_set", new()
            {
                ["user.id"] = request.UserId,
                ["rule_set.id"] = request.RuleSetId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetRuleSet";
            var filterIds = new Dictionary<string, object>
            {
                ["RuleSetId"] = request.RuleSetId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetRuleSet API Started", filterIds: filterIds);
                _tracer.AddEvent("get_rule_set_request_started");


                var result = await _workflowService.GetRuleSetAsync(request);

                _tracer.AddEvent("get_rule_set_completed_successfully");
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetRuleSet API Ended", filterIds: filterIds);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.AddEvent("get_rule_set_failed", new() { ["error"] = ex.Message });
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetRuleSet API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        [HttpPost("getDomain")]
        public async Task<IActionResult> GetDomain([FromBody] GetDomainRequest request)
        {
            using var span = _tracer.StartSpan("get_domain", new()
            {
                ["user.id"] = request.UserId,
                ["domain.id"] = request.DomainId?.ToString() ?? "NULL"
            });

            const string serviceName = "GetDomain";
            var filterIds = new Dictionary<string, object>
            {
                ["DomainId"] = request.DomainId?.ToString() ?? "NULL",
                ["OrgId"] = request.OrgId
            };

            try
            {
                _logger.LogInformation(serviceName, "GetDomain API Started", filterIds: filterIds);
                _tracer.AddEvent("get_domain_request_started");


                var result = await _workflowService.GetDomainAsync(request);

                _tracer.AddEvent("get_domain_completed_successfully");
                _tracer.SetStatus(ActivityStatusCode.Ok);
                _logger.LogInformation(serviceName, "GetDomain API Ended", filterIds: filterIds);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _tracer.AddEvent("get_domain_failed", new() { ["error"] = ex.Message });
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _logger.LogError(serviceName, "GetDomain API Error",
                    errorDetails: ex.Message, filterIds: filterIds, exception: ex);
                throw;
            }
        }
        /// <summary>
        /// Creates or updates a Workflow Rule Set. Calls sp_WF_CONFIG_Save_Rule_Set.
        /// </summary>
        [HttpPost("rule-set/save")]
        public async Task<IActionResult> SaveRuleSet([FromBody] SaveRuleSetRequest request)
        {
            string serviceName = "SaveRuleSet";
            var filterIds = new Dictionary<string, object> { { "RuleSetId", request.RuleSetId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveRuleSetAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Authentication Provider. Calls sp_WF_CONFIG_Save_Authentication_Provider.
        /// </summary>
        [HttpPost("authentication-provider/save")]
        public async Task<IActionResult> SaveAuthenticationProvider([FromBody] SaveAuthenticationProviderRequest request)
        {
            string serviceName = "SaveAuthenticationProvider";
            var filterIds = new Dictionary<string, object> { { "AuthenticationProviderId", request.AuthenticationProviderId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveAuthenticationProviderAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Authentication Policy. Calls sp_WF_CONFIG_Save_Authentication_Policy.
        /// </summary>
        [HttpPost("authentication-policy/save")]
        public async Task<IActionResult> SaveAuthenticationPolicy([FromBody] SaveAuthenticationPolicyRequest request)
        {
            string serviceName = "SaveAuthenticationPolicy";
            var filterIds = new Dictionary<string, object> { { "AuthenticationPolicyId", request.AuthenticationPolicyId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveAuthenticationPolicyAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates an Org-Level Default Policy mapping. Calls sp_WF_CONFIG_Save_Org_Level_Default_Policy.
        /// </summary>
        [HttpPost("org-level-default-policy/save")]
        public async Task<IActionResult> SaveOrgLevelDefaultPolicy([FromBody] SaveOrgLevelDefaultPolicyRequest request)
        {
            string serviceName = "SaveOrgLevelDefaultPolicy";
            var filterIds = new Dictionary<string, object> { { "OrgLevelDefaultPolicyId", request.OrgLevelDefaultPolicyId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveOrgLevelDefaultPolicyAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Domain. Calls sp_WF_CONFIG_Save_Domain.
        /// </summary>
        [HttpPost("domain/save")]
        public async Task<IActionResult> SaveDomain([FromBody] SaveDomainRequest request)
        {
            string serviceName = "SaveDomain";
            var filterIds = new Dictionary<string, object> { { "DomainId", request.DomainId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveDomainAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow SLA definition. Calls sp_WF_CONFIG_Save_SLA.
        /// </summary>
        [HttpPost("sla/save")]
        public async Task<IActionResult> SaveSla([FromBody] SaveSlaRequest request)
        {
            string serviceName = "SaveSla";
            var filterIds = new Dictionary<string, object> { { "SlaId", request.SlaId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveSlaAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Stage. Calls sp_WF_CONFIG_Save_Stage.
        /// </summary>
        [HttpPost("stage/save")]
        public async Task<IActionResult> SaveStage([FromBody] SaveStageRequest request)
        {
            string serviceName = "SaveStage";
            var filterIds = new Dictionary<string, object> { { "StageId", request.StageId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveStageAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Step. Calls sp_WF_CONFIG_Save_Step.
        /// </summary>
        [HttpPost("step/save")]
        public async Task<IActionResult> SaveStep([FromBody] SaveStepRequest request)
        {
            string serviceName = "SaveStep";
            var filterIds = new Dictionary<string, object> { { "StepId", request.StepId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveStepAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Task definition. Calls sp_WF_CONFIG_Save_Task.
        /// </summary>
        [HttpPost("task/save")]
        public async Task<IActionResult> SaveTask([FromBody] SaveTaskRequest request)
        {
            string serviceName = "SaveTask";
            var filterIds = new Dictionary<string, object> { { "TaskId", request.TaskId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveTaskAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Action definition. Calls sp_WF_CONFIG_Save_Action.
        /// </summary>
        [HttpPost("action/save")]
        public async Task<IActionResult> SaveAction([FromBody] SaveActionRequest request)
        {
            string serviceName = "SaveAction";
            var filterIds = new Dictionary<string, object> { { "ActionDefinitionId", request.ActionDefinitionId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveActionAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Entity definition. Calls sp_WF_CONFIG_Save_Entity.
        /// </summary>
        [HttpPost("entity/save")]
        public async Task<IActionResult> SaveEntity([FromBody] SaveEntityRequest request)
        {
            string serviceName = "SaveEntity";
            var filterIds = new Dictionary<string, object> { { "EntityDefinitionId", request.EntityDefinitionId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveEntityAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Entity Association. Calls sp_WF_CONFIG_Save_Entity_Association.
        /// </summary>
        [HttpPost("entity-association/save")]
        public async Task<IActionResult> SaveEntityAssociation([FromBody] SaveEntityAssociationRequest request)
        {
            string serviceName = "SaveEntityAssociation";
            var filterIds = new Dictionary<string, object> { { "WorkflowEntityAssociationId", request.WorkflowEntityAssociationId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveEntityAssociationAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Rule, including its Rule_CONFIG table-valued list.
        /// Calls sp_WF_CONFIG_Save_Rule.
        /// </summary>
        [HttpPost("rule/save")]
        public async Task<IActionResult> SaveRule([FromBody] SaveRuleRequest request)
        {
            string serviceName = "SaveRule";
            var filterIds = new Dictionary<string, object> { { "RuleId", request.RuleId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveRuleAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Authorization Policy, including its Authorization
        /// Policy Rule table-valued list. Calls sp_WF_CONFIG_Save_Authorization_Policy.
        /// </summary>
        [HttpPost("authorization-policy/save")]
        public async Task<IActionResult> SaveAuthorizationPolicy([FromBody] SaveAuthorizationPolicyRequest request)
        {
            string serviceName = "SaveAuthorizationPolicy";
            var filterIds = new Dictionary<string, object> { { "AuthorizationPolicyId", request.AuthorizationPolicyId ?? 0L } };

            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });

            try
            {
                var result = await _workflowService.SaveAuthorizationPolicyAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }
        protected async Task<IActionResult> CreateErrorResponse(Exception ex, ILoggingService logger, string serviceName, Dictionary<string, object>? filterIds = null)
        {    ErrorHandlingRequest errorRequest = new ErrorHandlingRequest
            {
                Exception = ex,
                ObservabilityAttributes = new DefaultObservabilityAttributes
                {
                    ServiceName = serviceName,
                    LogFilters = filterIds ?? new Dictionary<string, object>()
                }
            };
            var errorResponse = await _exceptionHandler.HandleExceptionAsync(errorRequest);

           

            return StatusCode(500, errorResponse);
        }

        /// <summary>
        /// Creates or updates a Workflow Event definition, including its
        /// Event Action table-valued list. Calls sp_WF_CONFIG_Save_Event.
        /// </summary>
        [HttpPost("event/save")]
        public async Task<IActionResult> SaveEvent([FromBody] SaveEventRequest request)
        {
            string serviceName = "SaveEvent";
            var filterIds = new Dictionary<string, object> { { "EventId", request.EventId ?? 0L } };
            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });
            try
            {
                var result = await _workflowService.SaveEventAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow Execution Policy, including its
        /// Execution Policy Config table-valued list. Calls sp_WF_CONFIG_Save_Execution_Policy.
        /// </summary>
        [HttpPost("execution-policy/save")]
        public async Task<IActionResult> SaveExecutionPolicy([FromBody] SaveExecutionPolicyRequest request)
        {
            string serviceName = "SaveExecutionPolicy";
            var filterIds = new Dictionary<string, object> { { "ExecutionPolicyId", request.ExecutionPolicyId ?? 0L } };
            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });
            try
            {
                var result = await _workflowService.SaveExecutionPolicyAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Creates or updates a Workflow definition together with its embedded
        /// Workflow Version. Calls sp_WF_CONFIG_Save_Workflow.
        /// </summary>
        [HttpPost("workflow/save")]
        public async Task<IActionResult> SaveWorkflow([FromBody] SaveWorkflowRequest request)
        {
            string serviceName = "SaveWorkflow";
            var filterIds = new Dictionary<string, object> { { "WorkflowId", request.WorkflowId ?? 0L } };
            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });
            try
            {
                var result = await _workflowService.SaveWorkflowAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }

        /// <summary>
        /// Replaces the full set of Workflow Transitions for a given Workflow Version.
        /// Calls sp_WF_CONFIG_Save_Workflow_Transitions.
        /// </summary>
        [HttpPost("workflow/transitions/save")]
        public async Task<IActionResult> SaveWorkflowTransitions([FromBody] SaveWorkflowTransitionsRequest request)
        {
            string serviceName = "SaveWorkflowTransitions";
            var filterIds = new Dictionary<string, object> { { "WorkflowVersionId", request.WorkflowVersionId } };
            _logger.LogInformation(serviceName, "API Started", filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request } });
            try
            {
                var result = await _workflowService.SaveWorkflowTransitionsAsync(request, CorrelationId);
                _logger.LogInformation(serviceName, "API Ended", filterIds: filterIds);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateErrorResponse(ex, _logger, serviceName, filterIds);
            }
        }
    }
}
