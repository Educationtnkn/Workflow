using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Domain.Entities.Workflow
{
    public class WorkflowBaseRequest
    {
        public int OrgId { get; set; }
        public int UserId { get; set; }
    }
    public class GetAuthenticationProviderRequest : WorkflowBaseRequest
    {
        public long? AuthenticationProviderId { get; set; }
        public long? DomainId { get; set; }
    }
    public class GetAuthenticationPolicyRequest : WorkflowBaseRequest
    {
        public long? AuthenticationPolicyId { get; set; }
        public long? DomainId { get; set; }
    }
    public class GetOrgLevelDefaultPolicyRequest : WorkflowBaseRequest
    {
        public long? OrgLevelDefaultPolicyId { get; set; }
    }
    public class GetSlaRequest : WorkflowBaseRequest
    {
        public long? SlaId { get; set; }
        public long? DomainId { get; set; }
    }
    public class GetAuthorizationPolicyRequest : WorkflowBaseRequest
    {
        public long? AuthorizationPolicyId { get; set; }
        public long? DomainId { get; set; }
    }
    public class GetStageRequest : WorkflowBaseRequest
    {
        public long? StageId { get; set; }
        public long? DomainId { get; set; }
    }
    public class GetStepRequest : WorkflowBaseRequest
    {
        public long? StepId { get; set; }
        public long? DomainId { get; set; }
    }
    public class GetTaskRequest : WorkflowBaseRequest
    {
        public long? TaskId { get; set; }
        public long? DomainId { get; set; }
    }
    public class GetActionRequest : WorkflowBaseRequest
    {
        public long? ActionId { get; set; }
        public long? DomainId { get; set; }
    }
    public class GetEntityRequest : WorkflowBaseRequest
    {
        public long? EntityId { get; set; }
        public long? DomainId { get; set; }
    }
    public class GetEntityAssociationRequest : WorkflowBaseRequest
    {
        public long? EntityAssociationId { get; set; }
    }
    public class GetRuleRequest : WorkflowBaseRequest
    {
        public long? RuleId { get; set; }
        public long? DomainId { get; set; }
    }
    public class GetRuleSetRequest : WorkflowBaseRequest
    {
        public long? RuleSetId { get; set; }
        public long? DomainId { get; set; }
    }
    public class GetDomainRequest : WorkflowBaseRequest
    {
        public long? DomainId { get; set; }
    }
    public class CommonDBResponse
    {
        public int OutputStatusCode { get; set; }
        public string? OutputStatusMsg { get; set; }
        public List<Dictionary<string,object>>? Id { get; set; }
    }
    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Rule Set.
    /// </summary>
    public class SaveRuleSetRequest
    {
        public long? RuleSetId { get; set; }
        public long? ParentRuleSetId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? RuleSetCode { get; set; }
        public string? RuleSetName { get; set; }
        public string? RuleSetDescription { get; set; }
        public string? RuleSetTypeCode { get; set; }
        public string? RuleSetCategoryCode { get; set; }
        public string? EvaluationModeCode { get; set; }
        public string? DefaultOutcomeCode { get; set; }
        public string? FailureHandlingModeCode { get; set; }
        public int? PriorityNumber { get; set; }
        public bool? IsReusableFlag { get; set; }
        public bool? IsSystemRuleSetFlag { get; set; }
        public string? RuleSetExpression { get; set; }
        public string? EvaluatorReference { get; set; }
        public DateTime? ValidFromDate { get; set; }
        public DateTime? ValidToDate { get; set; }
        public string? RuleSetConfigJson { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Authentication Provider.
    /// </summary>
    public class SaveAuthenticationProviderRequest
    {
        public long? AuthenticationProviderId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? ProviderCode { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderDescription { get; set; }
        public string? ProviderTypeCode { get; set; }
        public string? ProtocolTypeCode { get; set; }
        public string? AuthorityUrl { get; set; }
        public string? MetadataUrl { get; set; }
        public string? JwksUrl { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public string? ClientId { get; set; }
        public string? CredentialReference { get; set; }
        public string? CertificateThumbprint { get; set; }
        public string? TokenValidationModeCode { get; set; }
        public string? ProviderConfigJson { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Authentication Policy.
    /// </summary>
    public class SaveAuthenticationPolicyRequest
    {
        public long? AuthenticationPolicyId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public long? AuthenticationProviderId { get; set; }
        public string? AuthenticationPolicyCode { get; set; }
        public string? AuthenticationPolicyName { get; set; }
        public string? AuthenticationPolicyDescription { get; set; }
        public string? AuthenticationPolicyTypeCode { get; set; }
        public string? AuthenticationFlowCode { get; set; }
        public string? PrincipalTypeCode { get; set; }
        public bool? IsMfaRequiredFlag { get; set; }
        public bool? IsUserAuthenticationAllowedFlag { get; set; }
        public bool? IsServiceAuthenticationAllowedFlag { get; set; }
        public bool? IsAnonymousAllowedFlag { get; set; }
        public string? RequiredScopeExpression { get; set; }
        public string? RequiredClaimExpression { get; set; }
        public string? AllowedAudienceExpression { get; set; }
        public string? AllowedIssuerExpression { get; set; }
        public int? SessionTimeoutMinutes { get; set; }
        public int? MaxFailedAttemptCount { get; set; }
        public string? PolicyConfigJson { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) an Org-Level Default Policy mapping.
    /// </summary>
    public class SaveOrgLevelDefaultPolicyRequest
    {
        public long? OrgLevelDefaultPolicyId { get; set; }
        public int OrgId { get; set; }
        public long? DefaultAuthenticationPolicyId { get; set; }
        public long? DefaultAuthorizationPolicyId { get; set; }
        public long? DefaultSlaDefinitionId { get; set; }
        public long? DefaultExecutionPolicyId { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Domain.
    /// </summary>
    public class SaveDomainRequest
    {
        public long? DomainId { get; set; }
        public int OrgId { get; set; }
        public long? ParentDomainId { get; set; }
        public string? DomainCode { get; set; }
        public string? DomainName { get; set; }
        public string? DomainDescription { get; set; }
        public string? DomainTypeCode { get; set; }
        public string? DomainOwnerTeamCode { get; set; }
        public long? DefaultAuthenticationPolicyId { get; set; }
        public long? DefaultAuthorizationPolicyId { get; set; }
        public long? DefaultSlaDefinitionId { get; set; }
        public long? DefaultExecutionPolicyId { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow SLA definition.
    /// </summary>
    public class SaveSlaRequest
    {
        public long? SlaId { get; set; }
        public long? ParentSlaDefinitionId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? SlaCode { get; set; }
        public string? SlaName { get; set; }
        public string? SlaDescription { get; set; }
        public string? SlaTypeCode { get; set; }
        public string? SlaPriorityCode { get; set; }
        public string? TimeCalculationModeCode { get; set; }
        public string? BusinessCalendarReference { get; set; }
        public int? TargetDurationSeconds { get; set; }
        public int? WarningDurationSeconds { get; set; }
        public int? BreachDurationSeconds { get; set; }
        public int? EscalationAfterSeconds { get; set; }
        public string? EscalationPolicyReference { get; set; }
        public string? WarningActionCode { get; set; }
        public string? BreachActionCode { get; set; }
        public string? EscalationActionCode { get; set; }
        public string? PauseConditionExpression { get; set; }
        public string? ResumeConditionExpression { get; set; }
        public string? SlaOwnerTypeCode { get; set; }
        public string? SlaOwnerReference { get; set; }
        public bool? IsBreachBlockingFlag { get; set; }
        public bool? IsNotificationRequiredFlag { get; set; }
        public bool? IsReusableFlag { get; set; }
        public bool? IsSystemSlaFlag { get; set; }
        public DateTime? ValidFromDate { get; set; }
        public DateTime? ValidToDate { get; set; }
        public string? SlaConfigJson { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Stage.
    /// </summary>
    public class SaveStageRequest
    {
        public long? StageId { get; set; }
        public long? ParentStageId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? StageCode { get; set; }
        public string? StageName { get; set; }
        public string? StageDescription { get; set; }
        public long? AuthenticationPolicyId { get; set; }
        public long? AuthorizationPolicyId { get; set; }
        public string? RuleReferenceTypeCode { get; set; }
        public long? RuleReferenceId { get; set; }
        public long? EventId { get; set; }
        public long? SlaId { get; set; }
        public long? ExecutionPolicyId { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Step.
    /// </summary>
    public class SaveStepRequest
    {
        public long? StepId { get; set; }
        public long? ParentStepId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? StepCode { get; set; }
        public string? StepName { get; set; }
        public string? StepDescription { get; set; }
        public long? AuthenticationPolicyId { get; set; }
        public long? AuthorizationPolicyId { get; set; }
        public string? RuleReferenceTypeCode { get; set; }
        public long? RuleReferenceId { get; set; }
        public long? EventId { get; set; }
        public long? SlaId { get; set; }
        public long? ExecutionPolicyId { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Task definition.
    /// </summary>
    public class SaveTaskRequest
    {
        public long? TaskId { get; set; }
        public long? ParentTaskId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? TaskCode { get; set; }
        public string? TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public string? TaskTypeCode { get; set; }
        public string? TaskLevelCode { get; set; }
        public string? TaskExecutionModeCode { get; set; }
        public string? TaskOwnerTypeCode { get; set; }
        public string? TaskOwnerReference { get; set; }
        public long? AuthenticationPolicyId { get; set; }
        public long? AuthorizationPolicyId { get; set; }
        public string? RuleReferenceTypeCode { get; set; }
        public long? RuleReferenceId { get; set; }
        public long? EventId { get; set; }
        public long? SlaId { get; set; }
        public long? ExecutionPolicyId { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Action definition.
    /// </summary>
    public class SaveActionRequest
    {
        public long? ActionDefinitionId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? ActionCode { get; set; }
        public string? ActionName { get; set; }
        public string? ActionDescription { get; set; }
        public string? ActionTypeCode { get; set; }
        public string? ActionCategoryCode { get; set; }
        public string? ActionHandlerReference { get; set; }
        public string? ActionInputSchemaReference { get; set; }
        public string? ActionOutputSchemaReference { get; set; }
        public string? SuccessConditionExpression { get; set; }
        public string? FailureConditionExpression { get; set; }
        public long? AuthenticationPolicyId { get; set; }
        public long? AuthorizationPolicyId { get; set; }
        public string? RuleReferenceTypeCode { get; set; }
        public long? RuleReferenceId { get; set; }
        public long? EventId { get; set; }
        public long? SlaId { get; set; }
        public long? ExecutionPolicyId { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Entity definition.
    /// </summary>
    public class SaveEntityRequest
    {
        public long? EntityDefinitionId { get; set; }
        public long? ParentEntityDefinitionId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? EntityCode { get; set; }
        public string? EntityName { get; set; }
        public string? EntityDescription { get; set; }
        public string? EntityTypeCode { get; set; }
        public string? EntityCategoryCode { get; set; }
        public string? EntitySourceTypeCode { get; set; }
        public string? SourceSystemReference { get; set; }
        public string? LogicalEntityName { get; set; }
        public string? PhysicalEntityReference { get; set; }
        public string? PrimaryKeyAttributeName { get; set; }
        public string? DisplayAttributeName { get; set; }
        public string? EntitySchemaReference { get; set; }
        public string? DefaultContextPath { get; set; }
        public bool? IsCoreEntityFlag { get; set; }
        public bool? IsReusableFlag { get; set; }
        public bool? IsSystemEntityFlag { get; set; }
        public string? EntityConfigJson { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Entity Association.
    /// </summary>
    public class SaveEntityAssociationRequest
    {
        public long? WorkflowEntityAssociationId { get; set; }
        public long? WorkflowVersionId { get; set; }
        public string? AssociationLevelCode { get; set; }
        public long? AssociationReferenceId { get; set; }
        public long? EntityDefinitionId { get; set; }
        public string? EntityAlias { get; set; }
        public string? EntityRoleCode { get; set; }
        public string? EntityUsageCode { get; set; }
        public string? EntityAccessModeCode { get; set; }
        public string? CardinalityCode { get; set; }
        public bool? IsPrimaryEntityFlag { get; set; }
        public bool? IsRequiredFlag { get; set; }
        public bool? IsContextEntityFlag { get; set; }
        public string? ContextPath { get; set; }
        public string? EntityKeyContextPath { get; set; }
        public long? RelatedEntityDefinitionId { get; set; }
        public string? EntityRelationshipTypeCode { get; set; }
        public string? RelationshipExpression { get; set; }
        public string? FilterExpression { get; set; }
        public long? ValidationRuleSetId { get; set; }
        public int? SequenceNumber { get; set; }
        public string? AssociationConfigJson { get; set; }
        public short? StatusCode { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// A single Rule_CONFIG row (table-valued parameter row) used by <see cref="SaveRuleRequest"/>.
    /// </summary>
    public class RuleConfigItem
    {
        public long? RuleConfigId { get; set; }
        public string? ConfigKey { get; set; }
        public string? ConfigValue { get; set; }
        public short? ConfigDataTypeCode { get; set; }
        public short? ConfigValueUnitCode { get; set; }
        public short? ConfigCategoryCode { get; set; }
        public string? ConfigDescription { get; set; }
        public bool? IsRequiredFlag { get; set; }
        public bool? IsSensitiveFlag { get; set; }
        public int? DisplayOrder { get; set; }
        public string? ValidationExpression { get; set; }
        public string? DefaultValue { get; set; }
        public string? AllowedValues { get; set; }
        public short? StatusCode { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Rule, including its
    /// table-valued list of Rule_CONFIG key/value settings.
    /// </summary>
    public class SaveRuleRequest
    {
        public long? RuleId { get; set; }
        public long? RuleSetId { get; set; }
        public long? ParentRuleId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? RuleCode { get; set; }
        public string? RuleName { get; set; }
        public string? RuleDescription { get; set; }
        public string? RuleTypeCode { get; set; }
        public string? RuleCategoryCode { get; set; }
        public string? EvaluationTypeCode { get; set; }
        public string? EvaluatorReference { get; set; }
        public string? ConditionExpression { get; set; }
        public string? InputContextPath { get; set; }
        public string? OutputContextPath { get; set; }
        public string? SuccessOutcomeCode { get; set; }
        public string? FailureOutcomeCode { get; set; }
        public string? DefaultOutcomeCode { get; set; }
        public string? FailureHandlingModeCode { get; set; }
        public int? PriorityNumber { get; set; }
        public string? SeverityCode { get; set; }
        public bool? IsBlockingRuleFlag { get; set; }
        public bool? IsReusableFlag { get; set; }
        public bool? IsSystemRuleFlag { get; set; }
        public DateTime? ValidFromDate { get; set; }
        public DateTime? ValidToDate { get; set; }
        public string? RuleConfigJson { get; set; }
        public short? StatusCode { get; set; }
        public List<RuleConfigItem> RuleConfigList { get; set; } = new();
        public int UserId { get; set; }
    }

    /// <summary>
    /// A single Authorization Policy Rule row (table-valued parameter row) used by
    /// <see cref="SaveAuthorizationPolicyRequest"/>.
    /// </summary>
    public class AuthorizationPolicyRuleItem
    {
        public long? AuthorizationPolicyRuleId { get; set; }
        public string? AuthorizationRuleCode { get; set; }
        public string? AuthorizationRuleName { get; set; }
        public string? AuthorizationRuleDescription { get; set; }
        public short? PrincipalTypeCode { get; set; }
        public string? PrincipalReference { get; set; }
        public short? ResourceTypeCode { get; set; }
        public string? ResourceReference { get; set; }
        public short? ActionCode { get; set; }
        public short? PermissionEffectCode { get; set; }
        public string? ConditionExpression { get; set; }
        public int? EvaluationPriorityNumber { get; set; }
        public bool? IsDefaultRuleFlag { get; set; }
        public DateTime? ValidFromDate { get; set; }
        public DateTime? ValidToDate { get; set; }
        public string? ExternalRuleReference { get; set; }
        public string? RuleConfigJson { get; set; }
        public short? StatusCode { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Authorization Policy,
    /// including its table-valued list of Authorization Policy Rules.
    /// </summary>
    public class SaveAuthorizationPolicyRequest
    {
        public long? AuthorizationPolicyId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? AuthorizationPolicyCode { get; set; }
        public string? AuthorizationPolicyName { get; set; }
        public string? AuthorizationPolicyDescription { get; set; }
        public string? AuthorizationPolicyTypeCode { get; set; }
        public string? AuthorizationScopeCode { get; set; }
        public string? EvaluationModeCode { get; set; }
        public string? DefaultDecisionCode { get; set; }
        public int? PriorityNumber { get; set; }
        public string? ExternalPolicyReference { get; set; }
        public string? PolicyExpression { get; set; }
        public bool? IsExternalAuthorizationFlag { get; set; }
        public bool? IsSystemPolicyFlag { get; set; }
        public string? PolicyConfigJson { get; set; }
        public short? StatusCode { get; set; }
        public List<AuthorizationPolicyRuleItem> AuthorizationPolicyRuleList { get; set; } = new();
        public int UserId { get; set; }
    }
    /// <summary>
    /// A single Event_Action row (table-valued parameter row) used by <see cref="SaveEventRequest"/>.
    /// </summary>
    public class EventActionItem
    {
        public long? EventActionId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? EventActionCode { get; set; }
        public string? EventActionName { get; set; }
        public string? EventActionDescription { get; set; }
        public string? EventActionTypeCode { get; set; }
        public string? EventActionCategoryCode { get; set; }
        public string? EventActionReferenceTypeCode { get; set; }
        public long? EventActionReferenceId { get; set; }
        public string? EventActionReferenceValue { get; set; }
        public string? OutcomeCode { get; set; }
        public string? ConditionExpression { get; set; }
        public long? RuleSetId { get; set; }
        public long? ExecutionPolicyId { get; set; }
        public int? SequenceNumber { get; set; }
        public bool? IsMandatoryFlag { get; set; }
        public bool? ContinueOnFailureFlag { get; set; }
        public bool? IsIdempotentFlag { get; set; }
        public string? EventActionConfigJson { get; set; }
        public short? StatusCode { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Event definition,
    /// including its table-valued list of Event Actions.
    /// </summary>
    public class SaveEventRequest
    {
        public long? EventId { get; set; }
        public long? ParentEventDefinitionId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? EventCode { get; set; }
        public string? EventName { get; set; }
        public string? EventDescription { get; set; }
        public string? EventTypeCode { get; set; }
        public string? EventCategoryCode { get; set; }
        public string? EventSourceTypeCode { get; set; }
        public string? EventSourceReference { get; set; }
        public string? EventPayloadSchemaReference { get; set; }
        public string? EventCorrelationKeyExpression { get; set; }
        public string? EventFilterExpression { get; set; }
        public long? RuleSetId { get; set; }
        public int? TimeoutDurationSeconds { get; set; }
        public string? TimeoutActionCode { get; set; }
        public bool? IsReplayAllowedFlag { get; set; }
        public bool? IsDuplicateAllowedFlag { get; set; }
        public bool? IsEventPersistentFlag { get; set; }
        public string? EventConfigJson { get; set; }
        public short? StatusCode { get; set; }
        public List<EventActionItem> EventActionList { get; set; } = new();
        public int UserId { get; set; }
    }

    /// <summary>
    /// A single Execution_Policy_CONFIG row (table-valued parameter row) used by
    /// <see cref="SaveExecutionPolicyRequest"/>.
    /// </summary>
    public class ExecutionPolicyConfigItem
    {
        public long? ExecutionConfigId { get; set; }
        public string? ConfigKey { get; set; }
        public string? ConfigValue { get; set; }
        public string? ConfigDataTypeCode { get; set; }
        public string? ConfigValueUnitCode { get; set; }
        public string? ConfigCategoryCode { get; set; }
        public string? ConfigDescription { get; set; }
        public bool? IsRequiredFlag { get; set; }
        public bool? IsSensitiveFlag { get; set; }
        public int? DisplayOrder { get; set; }
        public string? ValidationExpression { get; set; }
        public string? DefaultValue { get; set; }
        public string? AllowedValues { get; set; }
        public string? EnvironmentScopeCode { get; set; }
        public short? StatusCode { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow Execution Policy,
    /// including its table-valued list of Execution_Policy_CONFIG key/value settings.
    /// </summary>
    public class SaveExecutionPolicyRequest
    {
        public long? ExecutionPolicyId { get; set; }
        public long? ParentExecutionPolicyId { get; set; }
        public int OrgId { get; set; }
        public long? DomainId { get; set; }
        public string? ExecutionPolicyCode { get; set; }
        public string? ExecutionPolicyName { get; set; }
        public string? ExecutionPolicyDescription { get; set; }
        public string? ExecutionPolicyTypeCode { get; set; }
        public string? ExecutionModeCode { get; set; }
        public string? ExecutionPriorityCode { get; set; }
        public int? RetryCount { get; set; }
        public int? RetryIntervalSeconds { get; set; }
        public string? RetryStrategyCode { get; set; }
        public int? MaxRetryIntervalSeconds { get; set; }
        public int? TimeoutSeconds { get; set; }
        public int? OverallTimeoutSeconds { get; set; }
        public string? FailureHandlingModeCode { get; set; }
        public string? TimeoutHandlingModeCode { get; set; }
        public string? SuccessHandlingModeCode { get; set; }
        public string? QueueReference { get; set; }
        public string? WorkerReference { get; set; }
        public int? MaxConcurrencyCount { get; set; }
        public int? BatchSize { get; set; }
        public int? RateLimitCount { get; set; }
        public int? RateLimitWindowSeconds { get; set; }
        public bool? IsIdempotencyRequiredFlag { get; set; }
        public string? IdempotencyKeyExpression { get; set; }
        public bool? IsCompensationRequiredFlag { get; set; }
        public string? CompensationActionReference { get; set; }
        public bool? IsTransactionRequiredFlag { get; set; }
        public string? TransactionScopeCode { get; set; }
        public bool? IsReusableFlag { get; set; }
        public bool? IsSystemPolicyFlag { get; set; }
        public string? ExecutionConfigJson { get; set; }
        public short? StatusCode { get; set; }
        public List<ExecutionPolicyConfigItem> ExecutionPolicyConfigList { get; set; } = new();
        public int UserId { get; set; }
    }

    /// <summary>
    /// Embedded Workflow Version data for <see cref="SaveWorkflowRequest"/>, corresponding
    /// to the @p_Version_* parameters of sp_WF_CONFIG_Save_Workflow.
    /// </summary>
    public class WorkflowVersionItem
    {
        public long? WorkflowVersionId { get; set; }
        public long? ParentWorkflowVersionId { get; set; }
        public int? OrgId { get; set; }
        public long? DomainId { get; set; }
        public long? AuthenticationPolicyId { get; set; }
        public long? AuthorizationPolicyId { get; set; }
        public string? RuleReferenceTypeCode { get; set; }
        public long? RuleReferenceId { get; set; }
        public long? EventId { get; set; }
        public long? SlaId { get; set; }
        public long? ExecutionPolicyId { get; set; }
        public string? VersionNumber { get; set; }
        public string? VersionName { get; set; }
        public string? VersionDescription { get; set; }
        public DateTime? EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }
        public bool? IsPublishedFlag { get; set; }
        public bool? IsCurrentFlag { get; set; }
        public DateTime? ValidatedDateTime { get; set; }
        public DateTime? PublishedDateTime { get; set; }
        public string? VersionStatusCode { get; set; }
    }

    /// <summary>
    /// Request payload for saving (creating or updating) a Workflow definition together
    /// with its embedded Workflow Version. Maps to sp_WF_CONFIG_Save_Workflow, which
    /// accepts both the Workflow_* parameters and the Version_* parameters in one call.
    /// </summary>
    public class SaveWorkflowRequest
    {
        public long? WorkflowId { get; set; }
        public long? ParentWorkflowId { get; set; }
        public string? WorkflowCode { get; set; }
        public string? WorkflowName { get; set; }
        public string? WorkflowDescription { get; set; }
        public string? WorkflowCategoryCode { get; set; }
        public string? WorkflowTypeCode { get; set; }
        public short? StatusCode { get; set; }
        public WorkflowVersionItem? WorkflowVersion { get; set; }
        public int UserId { get; set; }
    }

    /// <summary>
    /// A single Workflow_Transition row (table-valued parameter row) used by
    /// <see cref="SaveWorkflowTransitionsRequest"/>.
    /// </summary>
    public class WorkflowTransitionItem
    {
        public long? WorkflowTransitionId { get; set; }
        public string? FromNodeTypeCode { get; set; }
        public long? FromNodeId { get; set; }
        public string? ToNodeTypeCode { get; set; }
        public long? ToNodeId { get; set; }
        public string? TransitionCode { get; set; }
        public string? TransitionName { get; set; }
        public string? TransitionDescription { get; set; }
        public string? TransitionModeCode { get; set; }
        public string? TransitionPathTypeCode { get; set; }
        public int? SequenceNumber { get; set; }
        public string? RuleReferenceTypeCode { get; set; }
        public long? RuleReferenceId { get; set; }
        public string? BranchGroupCode { get; set; }
        public string? BranchCompletionPolicyCode { get; set; }
        public int? RequiredCompletionCount { get; set; }
        public long? EventId { get; set; }
        public int? DelayDurationSeconds { get; set; }
        public bool? IsDefaultPathFlag { get; set; }
        public bool? IsStartTransitionFlag { get; set; }
        public bool? IsTerminalTransitionFlag { get; set; }
        public short? StatusCode { get; set; }
    }

    /// <summary>
    /// Request payload for saving (replacing) the full set of Workflow Transitions for a
    /// given Workflow Version. Maps to sp_WF_CONFIG_Save_Workflow_Transitions, which is
    /// entirely table-valued: a Workflow_Version_ID plus the list of transitions.
    /// </summary>
    public class SaveWorkflowTransitionsRequest
    {
        public long WorkflowVersionId { get; set; }
        public List<WorkflowTransitionItem> TransitionList { get; set; } = new();
        public int UserId { get; set; }
    }
}
