using DB_Provider.Core.Domains;
using DB_Provider.Domain;
using DB_Provider.Interface;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Storage.Interfaces.Repositories;
using EnterpriseWorkflow.Utilities.DbConnectors;
using EnterpriseWorkflow.Utilities.Observability;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc.Filters;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Traces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Storage.Implementations.Repositories.SqlServer
{
    public class WorkflowCreationRepository : IWorkflowCreationRepository
    {
        private readonly ILoggingService _logger;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IMssqlExecutor _executor;
        private readonly INDTracerService _tracer;
        private readonly MSSQLConnector _mssqlConnector;
        private readonly SqlServerConnector2 _sqlServerConnector2;
        // ── Schema / SP name constants ────────────────────────────────────────
        private const string Schema = "Workflow";

        public WorkflowCreationRepository(
            ILoggingService logger,
            IDbConnectionFactory connectionFactory,
            IMssqlExecutor executor,
            INDTracerService tracer, MSSQLConnector mssqlConnector,
            SqlServerConnector2 sqlServerConnector2)
        {
            _mssqlConnector = mssqlConnector ?? throw new ArgumentNullException(nameof(mssqlConnector));
            _sqlServerConnector2 = sqlServerConnector2 ?? throw new ArgumentNullException(nameof(sqlServerConnector2));
            _logger = logger;
            _connectionFactory = connectionFactory;
            _executor = executor;
            _tracer = tracer;
        }

        public Task<List<List<Dictionary<string, object>>>> GetSlaAsync(GetSlaRequest request) =>
            ExecuteGetAsync(
                Guid.NewGuid().ToString(),
                "GetSla",
                "sp_WF_CONFIG_Get_SLA",
                new Dictionary<string, object>
                {
                    { "@p_SLA_ID",    (object?)request.SlaId    ?? DBNull.Value },
                    { "@p_Org_ID",    request.OrgId },
                    { "@p_Domain_ID", (object?)request.DomainId ?? DBNull.Value }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_SLA_ID",    SqlDbType.BigInt },
                    { "@p_Org_ID",    SqlDbType.Int    },
                    { "@p_Domain_ID", SqlDbType.BigInt }
                },
                new Dictionary<string, object>
                {
                    { "SlaId", request.SlaId?.ToString() ?? "NULL" },
                    { "OrgId", request.OrgId }
                });
        public Task<List<List<Dictionary<string, object>>>> GetAuthenticationProviderAsync(GetAuthenticationProviderRequest request) =>
                    ExecuteGetAsync(
                        Guid.NewGuid().ToString(),
                        "GetAuthenticationProvider",
                        "sp_WF_CONFIG_Get_Authentication_Provider",
                        new Dictionary<string, object>
                        {
                    { "@p_Authentication_Provider_ID", (object?)request.AuthenticationProviderId ?? DBNull.Value },
                    { "@p_Org_ID",                     request.OrgId },
                    { "@p_Domain_ID",                  (object?)request.DomainId ?? DBNull.Value }
                        },
                        new Dictionary<string, SqlDbType>
                        {
                    { "@p_Authentication_Provider_ID", SqlDbType.BigInt },
                    { "@p_Org_ID",                     SqlDbType.Int    },
                    { "@p_Domain_ID",                  SqlDbType.BigInt }
                        },
                        new Dictionary<string, object>
                        {
                    { "AuthenticationProviderId", request.AuthenticationProviderId?.ToString() ?? "NULL" },
                    { "OrgId", request.OrgId }
                        });
        public Task<List<List<Dictionary<string, object>>>> GetAuthenticationPolicyAsync(GetAuthenticationPolicyRequest request) =>
            ExecuteGetAsync(
                Guid.NewGuid().ToString(),
                "GetAuthenticationPolicy",
                "sp_WF_CONFIG_Get_Authentication_Policy",
                new Dictionary<string, object>
                {
                    { "@p_Authentication_Policy_ID", (object?)request.AuthenticationPolicyId ?? DBNull.Value },
                    { "@p_Org_ID",                   request.OrgId },
                    { "@p_Domain_ID",                (object?)request.DomainId ?? DBNull.Value }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_Authentication_Policy_ID", SqlDbType.BigInt },
                    { "@p_Org_ID",                   SqlDbType.Int    },
                    { "@p_Domain_ID",                SqlDbType.BigInt }
                },
                new Dictionary<string, object>
                {
                    { "AuthenticationPolicyId", request.AuthenticationPolicyId?.ToString() ?? "NULL" },
                    { "OrgId", request.OrgId }
                });

        public Task<List<List<Dictionary<string, object>>>> GetOrgLevelDefaultPolicyAsync(GetOrgLevelDefaultPolicyRequest request) =>
            ExecuteGetAsync(
                Guid.NewGuid().ToString(),
                "GetOrgLevelDefaultPolicy",
                "sp_WF_CONFIG_Get_Org_Level_Default_Policy",
                new Dictionary<string, object>
                {
                    { "@p_Org_Level_Default_Policy_ID", (object?)request.OrgLevelDefaultPolicyId ?? DBNull.Value },
                    { "@p_Org_ID",                      request.OrgId }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_Org_Level_Default_Policy_ID", SqlDbType.BigInt },
                    { "@p_Org_ID",                      SqlDbType.Int    }
                },
                new Dictionary<string, object>
                {
                    { "OrgLevelDefaultPolicyId", request.OrgLevelDefaultPolicyId?.ToString() ?? "NULL" },
                    { "OrgId", request.OrgId }
                });

        public Task<List<List<Dictionary<string, object>>>> GetAuthorizationPolicyAsync(GetAuthorizationPolicyRequest request) =>
            ExecuteGetAsync(Guid.NewGuid().ToString(), "GetAuthorizationPolicy", "sp_WF_CONFIG_Get_Authorization_Policy",
                new Dictionary<string, object>
                {
                    { "@p_Authorization_Policy_ID", (object?)request.AuthorizationPolicyId ?? DBNull.Value },
                    { "@p_Org_ID",                  request.OrgId },
                    { "@p_Domain_ID",               (object?)request.DomainId ?? DBNull.Value }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_Authorization_Policy_ID", SqlDbType.BigInt },
                    { "@p_Org_ID",                  SqlDbType.Int    },
                    { "@p_Domain_ID",               SqlDbType.BigInt }
                },
                new Dictionary<string, object> { { "AuthorizationPolicyId", request.AuthorizationPolicyId?.ToString() ?? "NULL" }, { "OrgId", request.OrgId } });


        public Task<List<List<Dictionary<string, object>>>> GetStageAsync(GetStageRequest request) =>
           ExecuteGetAsync(Guid.NewGuid().ToString(), "GetStage", "sp_WF_CONFIG_Get_Stage",
               new Dictionary<string, object>
               {
                    { "@p_Stage_ID",  (object?)request.StageId  ?? DBNull.Value },
                    { "@p_Org_ID",    request.OrgId },
                    { "@p_Domain_ID", (object?)request.DomainId ?? DBNull.Value }
               },
               new Dictionary<string, SqlDbType>
               {
                    { "@p_Stage_ID",  SqlDbType.BigInt },
                    { "@p_Org_ID",    SqlDbType.Int    },
                    { "@p_Domain_ID", SqlDbType.BigInt }
               },
               new Dictionary<string, object> { { "StageId", request.StageId?.ToString() ?? "NULL" }, { "OrgId", request.OrgId } });

        public Task<List<List<Dictionary<string, object>>>> GetStepAsync(GetStepRequest request) =>
            ExecuteGetAsync(Guid.NewGuid().ToString(), "GetStep", "sp_WF_CONFIG_Get_Step",
                new Dictionary<string, object>
                {
                    { "@p_Step_ID",   (object?)request.StepId   ?? DBNull.Value },
                    { "@p_Org_ID",    request.OrgId },
                    { "@p_Domain_ID", (object?)request.DomainId ?? DBNull.Value }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_Step_ID",   SqlDbType.BigInt },
                    { "@p_Org_ID",    SqlDbType.Int    },
                    { "@p_Domain_ID", SqlDbType.BigInt }
                },
                new Dictionary<string, object> { { "StepId", request.StepId?.ToString() ?? "NULL" }, { "OrgId", request.OrgId } });

        public Task<List<List<Dictionary<string, object>>>> GetTaskAsync(GetTaskRequest request) =>
            ExecuteGetAsync(Guid.NewGuid().ToString(), "GetTask", "sp_WF_CONFIG_Get_Task",
                new Dictionary<string, object>
                {
                    { "@p_Task_ID",   (object?)request.TaskId   ?? DBNull.Value },
                    { "@p_Org_ID",    request.OrgId },
                    { "@p_Domain_ID", (object?)request.DomainId ?? DBNull.Value }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_Task_ID",   SqlDbType.BigInt },
                    { "@p_Org_ID",    SqlDbType.Int    },
                    { "@p_Domain_ID", SqlDbType.BigInt }
                },
                new Dictionary<string, object> { { "TaskId", request.TaskId?.ToString() ?? "NULL" }, { "OrgId", request.OrgId } });

        public Task<List<List<Dictionary<string, object>>>> GetActionAsync(GetActionRequest request) =>
            ExecuteGetAsync(Guid.NewGuid().ToString(), "GetAction", "sp_WF_CONFIG_Get_Action",
                new Dictionary<string, object>
                {
                    { "@p_Action_Definition_ID", (object?)request.ActionId ?? DBNull.Value },
                    { "@p_Org_ID",    request.OrgId },
                    { "@p_Domain_ID", (object?)request.DomainId ?? DBNull.Value }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_Action_Definition_ID", SqlDbType.BigInt },
                    { "@p_Org_ID",    SqlDbType.Int    },
                    { "@p_Domain_ID", SqlDbType.BigInt }
                },
                 new Dictionary<string, object> { { "ActionId", request.ActionId?.ToString() ?? "NULL" }, { "OrgId", request.OrgId } });

        public Task<List<List<Dictionary<string, object>>>> GetEntityAsync(GetEntityRequest request) =>
            ExecuteGetAsync(Guid.NewGuid().ToString(), "GetEntity", "sp_WF_CONFIG_Get_Entity",
                new Dictionary<string, object>
                {
                    { "@p_Entity_Definition_ID", (object?)request.EntityId ?? DBNull.Value },
                    { "@p_Org_ID",    request.OrgId },
                    { "@p_Domain_ID", (object?)request.DomainId ?? DBNull.Value }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_Entity_Definition_ID", SqlDbType.BigInt },
                    { "@p_Org_ID",    SqlDbType.Int    },
                    { "@p_Domain_ID", SqlDbType.BigInt }
                },
                 new Dictionary<string, object> { { "EntityId", request.EntityId?.ToString() ?? "NULL" }, { "OrgId", request.OrgId } });

        public Task<List<List<Dictionary<string, object>>>> GetEntityAssociationAsync(GetEntityAssociationRequest request) =>
            ExecuteGetAsync(Guid.NewGuid().ToString(), "GetEntityAssociation", "sp_WF_CONFIG_Get_Entity_Association",
                new Dictionary<string, object>
                {
                    { "@p_Workflow_Entity_Association_ID ", (object?)request.EntityAssociationId ?? DBNull.Value }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_Workflow_Entity_Association_ID ", SqlDbType.BigInt }
                },
                 new Dictionary<string, object> { { "EntityAssociationId", request.EntityAssociationId?.ToString() ?? "NULL" }, { "OrgId", request.OrgId } });

        public Task<List<List<Dictionary<string, object>>>> GetRuleAsync(GetRuleRequest request) =>
            ExecuteGetAsync(Guid.NewGuid().ToString(), "GetRule", "sp_WF_CONFIG_Get_Rule",
                new Dictionary<string, object>
                {
                    { "@p_Rule_ID",   (object?)request.RuleId   ?? DBNull.Value },
                    { "@p_Org_ID",    request.OrgId },
                    { "@p_Domain_ID", (object?)request.DomainId ?? DBNull.Value }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_Rule_ID",   SqlDbType.BigInt },
                    { "@p_Org_ID",    SqlDbType.Int    },
                    { "@p_Domain_ID", SqlDbType.BigInt }
                },
                 new Dictionary<string, object> { { "RuleId", request.RuleId?.ToString() ?? "NULL" }, { "OrgId", request.OrgId } });

        public Task<List<List<Dictionary<string, object>>>> GetRuleSetAsync(GetRuleSetRequest request) =>
            ExecuteGetAsync(
                Guid.NewGuid().ToString(),
                "GetRuleSet",
                "sp_WF_CONFIG_Get_Rule_Set",
                new Dictionary<string, object>
                {
                    { "@p_Rule_Set_ID", (object?)request.RuleSetId ?? DBNull.Value },
                    { "@p_Org_ID",      request.OrgId },
                    { "@p_Domain_ID",   (object?)request.DomainId ?? DBNull.Value }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_Rule_Set_ID", SqlDbType.BigInt },
                    { "@p_Org_ID",      SqlDbType.Int    },
                    { "@p_Domain_ID",   SqlDbType.BigInt }
                },
                
                new Dictionary<string, object>
                {
                    { "RuleSetId", request.RuleSetId?.ToString() ?? "NULL" },
                    { "OrgId",     request.OrgId }
                });
        public Task<List<List<Dictionary<string, object>>>> GetDomainAsync(GetDomainRequest request) =>
            ExecuteGetAsync(
                Guid.NewGuid().ToString(),
                "GetDomain",
                "sp_WF_CONFIG_Get_Domain",
                new Dictionary<string, object>
                {
                    { "@p_Domain_ID", (object?)request.DomainId ?? DBNull.Value },
                    { "@p_Org_ID",    request.OrgId }
                },
                new Dictionary<string, SqlDbType>
                {
                    { "@p_Domain_ID", SqlDbType.BigInt },
                    { "@p_Org_ID",    SqlDbType.Int    }
                },
                new Dictionary<string, object>
                {
                    { "DomainId", request.DomainId?.ToString() ?? "NULL" },
                    { "OrgId", request.OrgId }
                });
        //  Shared execution engine
        // =====================================================================

        private async Task<List<List<Dictionary<string, object>>>> ExecuteGetAsync(
            string? correlationId,
            string serviceName,
            string spName,
            Dictionary<string, object> inputParams,
            Dictionary<string, SqlDbType> inputParamTypes,
            Dictionary<string, object> logFilters)
        {
            var context = new DefaultLogContext
            {
                MethodName = spName,
                ServiceName = serviceName,
                CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
                LogFilters = logFilters
            };

            try
            {
                _tracer.StartSpan(serviceName, new Dictionary<string, object>(logFilters));

                _logger.LogInformation(serviceName,
                    $"Starting {serviceName} database operation", filterIds: logFilters);

                var connector = _connectionFactory.Create(DbProviderType.MSSQL);
                await using var connection = await connector.GetConnectionAsync("db");

                var spRequest = new MssqlExecutionRequest
                {
                    OperationName = spName,
                    SchemaName = Schema,
                    InputParameters = inputParams,
                    InputParameterTypes = inputParamTypes,
                    OutputParameters = new Dictionary<string, SqlDbType>
                    {
                        { "@p_Output_Status_Code", SqlDbType.Int      },
                        { "@p_Output_Status_Msg",  SqlDbType.NVarChar }
                    },
                    OutputParameterSizes = new Dictionary<string, int>
                    {
                        { "@p_Output_Status_Code", 100  },
                        { "@p_Output_Status_Msg",  4000 }
                    },
                    LogInfo = new MssqlExecutionRequest.LoggingDetails
                    {
                        ServiceName = serviceName,
                        CorrelationId = context.CorrelationId
                    }
                };

                _tracer.StartChildSpan("ExecuteStoredProcedure", new Dictionary<string, object>
                {
                    { "sp.name",   spName  },
                    { "sp.schema", Schema  }
                });

                await connection.OpenAsync();
                var result = _executor.ExecuteStoredProcedure(connection, spRequest);

                if (!result.Success)
                {
                    var errMsg = result.ErrorMessage ?? result.Message ?? "Database operation failed";
                    var errCode = result.ErrorCode ?? "DB_ERROR";

                    _logger.LogError(serviceName,
                        $"Database error in {serviceName} — Code: {errCode}, Msg: {errMsg}",
                        filterIds: logFilters);

                    _tracer.SetStatus(ActivityStatusCode.Error, errMsg);
                    throw new InvalidOperationException(
                        $"Database error in {serviceName}. ErrorCode: {errCode}, Message: {errMsg}");
                }

                var outputStatusCode = result.OutputParameters?.GetValueOrDefault("@p_Output_Status_Code")?.ToString();
                var outputStatusMsg = result.OutputParameters?.GetValueOrDefault("@p_Output_Status_Msg")?.ToString();

                _tracer.AddEvent($"{serviceName.ToLower()}_sp_output",
                    new Dictionary<string, object>
                    {
                        { "sp.output.status.code", outputStatusCode ?? "NULL" },
                        { "sp.output.status.msg",  outputStatusMsg  ?? "NULL" }
                    });

                if (!string.IsNullOrEmpty(outputStatusCode)
                    && outputStatusCode != "0"
                    && !outputStatusCode.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogError(serviceName,
                        $"SP [{Schema}].[{spName}] failed — StatusCode: {outputStatusCode}, Msg: {outputStatusMsg}",
                        filterIds: logFilters);

                    throw new InvalidOperationException(
                        $"SP [{Schema}].[{spName}] failed. StatusCode: {outputStatusCode}, Message: {outputStatusMsg}");
                }

                var response = result.ResultSets;

                _logger.LogInformation(serviceName,
                    $"{serviceName} Repository Completed — Records: {response.Count}",
                    filterIds: logFilters);

                _tracer.SetStatus(ActivityStatusCode.Ok);

                return response;
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError(serviceName, $"Null argument in {serviceName}",
                    filterIds: logFilters, exception: ex);
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(serviceName, $"Unexpected error in {serviceName}",
                    filterIds: logFilters, exception: ex);
                _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
                _tracer.AddEvent("Unexpected error", new Dictionary<string, object>
                {
                    { "error.type",    ex.GetType().Name },
                    { "error.message", ex.Message        }
                });
                throw;
            }
        }

        //public async Task<List<List<Dictionary<string,object>>>> GetStageAsync(GetStageRequest request)
        //{
        //    var serviceName = "GetStage";
        //    var logFilters = new Dictionary<string, object>
        //    {
        //        ["StageId"] = request.StageId?.ToString() ?? "NULL",
        //        ["OrgId"] = request.OrgId
        //    };
        //    var context = new DefaultLogContext
        //    {
        //        MethodName = "GetStageAsync",
        //        ServiceName = "GetStage",
        //        CorrelationId = request.CorrelationId ?? Guid.NewGuid().ToString()
        //    };

        //    try
        //    {
        //        _tracer.StartSpan(serviceName, new Dictionary<string, object>(logFilters));

        //        _logger.LogInformation(serviceName,
        //            $"Starting {serviceName} database operation", filterIds: logFilters);

        //        var connector = _connectionFactory.Create(DbProviderType.MSSQL);
        //        await using var connection = await connector.GetConnectionAsync("db");

        //        var spRequest = new MssqlExecutionRequest
        //        {
        //            OperationName = "sp_WF_CONFIG_Get_Stage",
        //            SchemaName = Schema,
        //            InputParameters = new Dictionary<string, object>
        //               {
        //                    { "@p_Stage_ID",  request.StageId},
        //                    { "@p_Org_ID",    request.OrgId },
        //                    { "@p_Domain_ID", request.DomainId }
        //               },
        //            InputParameterTypes = new Dictionary<string, SqlDbType>
        //               {
        //                    { "@p_Stage_ID",  SqlDbType.BigInt },
        //                    { "@p_Org_ID",    SqlDbType.Int    },
        //                    { "@p_Domain_ID", SqlDbType.BigInt }
        //               },
        //            OutputParameters = new Dictionary<string, SqlDbType>
        //            {
        //                { "@p_Output_Status_Code", SqlDbType.Int      },
        //                { "@p_Output_Status_Msg",  SqlDbType.NVarChar }
        //            },
        //            OutputParameterSizes = new Dictionary<string, int>
        //            {
        //                { "@p_Output_Status_Code", 100  },
        //                { "@p_Output_Status_Msg",  4000 }
        //            },
        //            LogInfo = new MssqlExecutionRequest.LoggingDetails
        //            {
        //                ServiceName = serviceName,
        //                CorrelationId = context.CorrelationId
        //            }
        //        };

        //        _tracer.StartChildSpan("ExecuteStoredProcedure", new Dictionary<string, object>
        //        {
        //            { "sp.name",   "sp_WF_CONFIG_Get_Stage"  },
        //            { "sp.schema", Schema  }
        //        });

        //        await connection.OpenAsync();
        //        var result = _executor.ExecuteStoredProcedure(connection, spRequest);

        //        if (!result.Success)
        //        {
        //            var errMsg = result.ErrorMessage ?? result.Message ?? "Database operation failed";
        //            var errCode = result.ErrorCode ?? "DB_ERROR";

        //            _logger.LogError(serviceName,
        //                $"Database error in {serviceName} — Code: {errCode}, Msg: {errMsg}",
        //                filterIds: logFilters);

        //            _tracer.SetStatus(ActivityStatusCode.Error, errMsg);
        //            throw new InvalidOperationException(
        //                $"Database error in {serviceName}. ErrorCode: {errCode}, Message: {errMsg}");
        //        }

        //        var outputStatusCode = result.OutputParameters?.GetValueOrDefault("@p_Output_Status_Code")?.ToString();
        //        var outputStatusMsg = result.OutputParameters?.GetValueOrDefault("@p_Output_Status_Msg")?.ToString();

        //        _tracer.AddEvent($"{serviceName.ToLower()}_sp_output",
        //            new Dictionary<string, object>
        //            {
        //                { "sp.output.status.code", outputStatusCode ?? "NULL" },
        //                { "sp.output.status.msg",  outputStatusMsg  ?? "NULL" }
        //            });

        //        if (!string.IsNullOrEmpty(outputStatusCode)
        //            && outputStatusCode != "0"
        //            && !outputStatusCode.Equals("SUCCESS", StringComparison.OrdinalIgnoreCase))
        //        {
        //            _logger.LogError(serviceName,
        //                $"SP [{Schema}].[sp_WF_CONFIG_Get_Stage] failed — StatusCode: {outputStatusCode}, Msg: {outputStatusMsg}",
        //                filterIds: logFilters);

        //            throw new InvalidOperationException(
        //                $"SP [{Schema}].[sp_WF_CONFIG_Get_Stage] failed. StatusCode: {outputStatusCode}, Message: {outputStatusMsg}");
        //        }

        //        var response = result.ResultSets;

        //        _logger.LogInformation(serviceName,
        //            $"{serviceName} Repository Completed — Records: {response.Count}",
        //            filterIds: logFilters);

        //        _tracer.SetStatus(ActivityStatusCode.Ok);

        //        return response;
        //    }
        //    catch (ArgumentNullException ex)
        //    {
        //        _logger.LogError(serviceName, $"Null argument in {serviceName}",
        //            filterIds: logFilters, exception: ex);
        //        _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(serviceName, $"Unexpected error in {serviceName}",
        //            filterIds: logFilters, exception: ex);
        //        _tracer.SetStatus(ActivityStatusCode.Error, ex.Message);
        //        _tracer.AddEvent("Unexpected error", new Dictionary<string, object>
        //        {
        //            { "error.type",    ex.GetType().Name },
        //            { "error.message", ex.Message        }
        //        });
        //        throw;
        //    }
        //}


        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Rule_Set to create or update a RuleSet record.
        /// </summary>
        public async Task<CommonDBResponse> SaveRuleSetAsync(SaveRuleSetRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveRuleSetAsync";
            var filterIds = new Dictionary<string, object> { { "RuleSetId", request.RuleSetId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_Rule_Set_ID", request.RuleSetId ?? (object)DBNull.Value },
                    { "@p_Parent_Rule_Set_ID", request.ParentRuleSetId ?? (object)DBNull.Value },
                    { "@p_Org_ID", request.OrgId },
                    { "@p_Domain_ID", request.DomainId ?? (object)DBNull.Value },
                    { "@p_Rule_Set_Code", request.RuleSetCode ?? (object)DBNull.Value },
                    { "@p_Rule_Set_Name", request.RuleSetName ?? (object)DBNull.Value },
                    { "@p_Rule_Set_Description", request.RuleSetDescription ?? (object)DBNull.Value },
                    { "@p_Rule_Set_Type_Code", request.RuleSetTypeCode ?? (object)DBNull.Value },
                    { "@p_Rule_Set_Category_Code", request.RuleSetCategoryCode ?? (object)DBNull.Value },
                    { "@p_Evaluation_Mode_Code", request.EvaluationModeCode ?? (object)DBNull.Value },
                    { "@p_Default_Outcome_Code", request.DefaultOutcomeCode ?? (object)DBNull.Value },
                    { "@p_Failure_Handling_Mode_Code", request.FailureHandlingModeCode ?? (object)DBNull.Value },
                    { "@p_Priority_Number", request.PriorityNumber ?? (object)DBNull.Value },
                    { "@p_Is_Reusable_Flag", request.IsReusableFlag ?? (object)DBNull.Value },
                    { "@p_Is_System_Rule_Set_Flag", request.IsSystemRuleSetFlag ?? (object)DBNull.Value },
                    { "@p_Rule_Set_Expression", request.RuleSetExpression ?? (object)DBNull.Value },
                    { "@p_Evaluator_Reference", request.EvaluatorReference ?? (object)DBNull.Value },
                    { "@p_Valid_From_Date", request.ValidFromDate ?? (object)DBNull.Value },
                    { "@p_Valid_To_Date", request.ValidToDate ?? (object)DBNull.Value },
                    { "@p_Rule_Set_CONFIG_Json", request.RuleSetConfigJson ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId },
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_Rule_Set_ID", SqlDbType.BigInt },
                    { "@p_Parent_Rule_Set_ID", SqlDbType.BigInt },
                    { "@p_Org_ID", SqlDbType.Int },
                    { "@p_Domain_ID", SqlDbType.BigInt },
                    { "@p_Rule_Set_Code", SqlDbType.NVarChar },
                    { "@p_Rule_Set_Name", SqlDbType.NVarChar },
                    { "@p_Rule_Set_Description", SqlDbType.NVarChar },
                    { "@p_Rule_Set_Type_Code", SqlDbType.NVarChar },
                    { "@p_Rule_Set_Category_Code", SqlDbType.NVarChar },
                    { "@p_Evaluation_Mode_Code", SqlDbType.NVarChar },
                    { "@p_Default_Outcome_Code", SqlDbType.NVarChar },
                    { "@p_Failure_Handling_Mode_Code", SqlDbType.NVarChar },
                    { "@p_Priority_Number", SqlDbType.Int },
                    { "@p_Is_Reusable_Flag", SqlDbType.Bit },
                    { "@p_Is_System_Rule_Set_Flag", SqlDbType.Bit },
                    { "@p_Rule_Set_Expression", SqlDbType.NVarChar },
                    { "@p_Evaluator_Reference", SqlDbType.NVarChar },
                    { "@p_Valid_From_Date", SqlDbType.DateTime },
                    { "@p_Valid_To_Date", SqlDbType.DateTime },
                    { "@p_Rule_Set_CONFIG_Json", SqlDbType.NVarChar },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int },
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_Rule_Set",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Authentication_Provider to create or update a AuthenticationProvider record.
        /// </summary>
        public async Task<CommonDBResponse> SaveAuthenticationProviderAsync(SaveAuthenticationProviderRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveAuthenticationProviderAsync";
            var filterIds = new Dictionary<string, object> { { "AuthenticationProviderId", request.AuthenticationProviderId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_Authentication_Provider_ID", request.AuthenticationProviderId ?? (object)DBNull.Value },
                    { "@p_Org_ID", request.OrgId },
                    { "@p_Domain_ID", request.DomainId ?? (object)DBNull.Value },
                    { "@p_Provider_Code", request.ProviderCode ?? (object)DBNull.Value },
                    { "@p_Provider_Name", request.ProviderName ?? (object)DBNull.Value },
                    { "@p_Provider_Description", request.ProviderDescription ?? (object)DBNull.Value },
                    { "@p_Provider_Type_Code", request.ProviderTypeCode ?? (object)DBNull.Value },
                    { "@p_Protocol_Type_Code", request.ProtocolTypeCode ?? (object)DBNull.Value },
                    { "@p_Authority_Url", request.AuthorityUrl ?? (object)DBNull.Value },
                    { "@p_Metadata_Url", request.MetadataUrl ?? (object)DBNull.Value },
                    { "@p_Jwks_Url", request.JwksUrl ?? (object)DBNull.Value },
                    { "@p_Issuer", request.Issuer ?? (object)DBNull.Value },
                    { "@p_Audience", request.Audience ?? (object)DBNull.Value },
                    { "@p_Client_ID", request.ClientId ?? (object)DBNull.Value },
                    { "@p_Credential_Reference", request.CredentialReference ?? (object)DBNull.Value },
                    { "@p_Certificate_Thumbprint", request.CertificateThumbprint ?? (object)DBNull.Value },
                    { "@p_Token_Validation_Mode_Code", request.TokenValidationModeCode ?? (object)DBNull.Value },
                    { "@p_Provider_CONFIG_Json", request.ProviderConfigJson ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId },
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_Authentication_Provider_ID", SqlDbType.BigInt },
                    { "@p_Org_ID", SqlDbType.Int },
                    { "@p_Domain_ID", SqlDbType.BigInt },
                    { "@p_Provider_Code", SqlDbType.NVarChar },
                    { "@p_Provider_Name", SqlDbType.NVarChar },
                    { "@p_Provider_Description", SqlDbType.NVarChar },
                    { "@p_Provider_Type_Code", SqlDbType.NVarChar },
                    { "@p_Protocol_Type_Code", SqlDbType.NVarChar },
                    { "@p_Authority_Url", SqlDbType.NVarChar },
                    { "@p_Metadata_Url", SqlDbType.NVarChar },
                    { "@p_Jwks_Url", SqlDbType.NVarChar },
                    { "@p_Issuer", SqlDbType.NVarChar },
                    { "@p_Audience", SqlDbType.NVarChar },
                    { "@p_Client_ID", SqlDbType.NVarChar },
                    { "@p_Credential_Reference", SqlDbType.NVarChar },
                    { "@p_Certificate_Thumbprint", SqlDbType.NVarChar },
                    { "@p_Token_Validation_Mode_Code", SqlDbType.NVarChar },
                    { "@p_Provider_CONFIG_Json", SqlDbType.NVarChar },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int },
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_Authentication_Provider",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Authentication_Policy to create or update a AuthenticationPolicy record.
        /// </summary>
        public async Task<CommonDBResponse> SaveAuthenticationPolicyAsync(SaveAuthenticationPolicyRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveAuthenticationPolicyAsync";
            var filterIds = new Dictionary<string, object> { { "AuthenticationPolicyId", request.AuthenticationPolicyId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_Authentication_Policy_ID", request.AuthenticationPolicyId ?? (object)DBNull.Value },
                    { "@p_Org_ID", request.OrgId },
                    { "@p_Domain_ID", request.DomainId ?? (object)DBNull.Value },
                    { "@p_Authentication_Provider_ID", request.AuthenticationProviderId ?? (object)DBNull.Value },
                    { "@p_Authentication_Policy_Code", request.AuthenticationPolicyCode ?? (object)DBNull.Value },
                    { "@p_Authentication_Policy_Name", request.AuthenticationPolicyName ?? (object)DBNull.Value },
                    { "@p_Authentication_Policy_Description", request.AuthenticationPolicyDescription ?? (object)DBNull.Value },
                    { "@p_Authentication_Policy_Type_Code", request.AuthenticationPolicyTypeCode ?? (object)DBNull.Value },
                    { "@p_Authentication_Flow_Code", request.AuthenticationFlowCode ?? (object)DBNull.Value },
                    { "@p_Principal_Type_Code", request.PrincipalTypeCode ?? (object)DBNull.Value },
                    { "@p_Is_Mfa_Required_Flag", request.IsMfaRequiredFlag ?? (object)DBNull.Value },
                    { "@p_Is_User_Authentication_Allowed_Flag", request.IsUserAuthenticationAllowedFlag ?? (object)DBNull.Value },
                    { "@p_Is_Service_Authentication_Allowed_Flag", request.IsServiceAuthenticationAllowedFlag ?? (object)DBNull.Value },
                    { "@p_Is_Anonymous_Allowed_Flag", request.IsAnonymousAllowedFlag ?? (object)DBNull.Value },
                    { "@p_Required_Scope_Expression", request.RequiredScopeExpression ?? (object)DBNull.Value },
                    { "@p_Required_Claim_Expression", request.RequiredClaimExpression ?? (object)DBNull.Value },
                    { "@p_Allowed_Audience_Expression", request.AllowedAudienceExpression ?? (object)DBNull.Value },
                    { "@p_Allowed_Issuer_Expression", request.AllowedIssuerExpression ?? (object)DBNull.Value },
                    { "@p_Session_Timeout_Minutes", request.SessionTimeoutMinutes ?? (object)DBNull.Value },
                    { "@p_Max_Failed_Attempt_Count", request.MaxFailedAttemptCount ?? (object)DBNull.Value },
                    { "@p_Policy_CONFIG_Json", request.PolicyConfigJson ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId },
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_Authentication_Policy_ID", SqlDbType.BigInt },
                    { "@p_Org_ID", SqlDbType.Int },
                    { "@p_Domain_ID", SqlDbType.BigInt },
                    { "@p_Authentication_Provider_ID", SqlDbType.BigInt },
                    { "@p_Authentication_Policy_Code", SqlDbType.NVarChar },
                    { "@p_Authentication_Policy_Name", SqlDbType.NVarChar },
                    { "@p_Authentication_Policy_Description", SqlDbType.NVarChar },
                    { "@p_Authentication_Policy_Type_Code", SqlDbType.NVarChar },
                    { "@p_Authentication_Flow_Code", SqlDbType.NVarChar },
                    { "@p_Principal_Type_Code", SqlDbType.NVarChar },
                    { "@p_Is_Mfa_Required_Flag", SqlDbType.Bit },
                    { "@p_Is_User_Authentication_Allowed_Flag", SqlDbType.Bit },
                    { "@p_Is_Service_Authentication_Allowed_Flag", SqlDbType.Bit },
                    { "@p_Is_Anonymous_Allowed_Flag", SqlDbType.Bit },
                    { "@p_Required_Scope_Expression", SqlDbType.NVarChar },
                    { "@p_Required_Claim_Expression", SqlDbType.NVarChar },
                    { "@p_Allowed_Audience_Expression", SqlDbType.NVarChar },
                    { "@p_Allowed_Issuer_Expression", SqlDbType.NVarChar },
                    { "@p_Session_Timeout_Minutes", SqlDbType.Int },
                    { "@p_Max_Failed_Attempt_Count", SqlDbType.Int },
                    { "@p_Policy_CONFIG_Json", SqlDbType.NVarChar },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int },
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_Authentication_Policy",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Org_Level_Default_Policy to create or update a OrgLevelDefaultPolicy record.
        /// </summary>
        public async Task<CommonDBResponse> SaveOrgLevelDefaultPolicyAsync(SaveOrgLevelDefaultPolicyRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveOrgLevelDefaultPolicyAsync";
            var filterIds = new Dictionary<string, object> { { "OrgLevelDefaultPolicyId", request.OrgLevelDefaultPolicyId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_Org_Level_Default_Policy_ID", request.OrgLevelDefaultPolicyId ?? (object)DBNull.Value },
                    { "@p_Org_ID", request.OrgId },
                    { "@p_Default_Authentication_Policy_ID", request.DefaultAuthenticationPolicyId ?? (object)DBNull.Value },
                    { "@p_Default_Authorization_Policy_ID", request.DefaultAuthorizationPolicyId ?? (object)DBNull.Value },
                    { "@p_Default_SLA_Definition_ID", request.DefaultSlaDefinitionId ?? (object)DBNull.Value },
                    { "@p_Default_Execution_Policy_ID", request.DefaultExecutionPolicyId ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId },
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_Org_Level_Default_Policy_ID", SqlDbType.BigInt },
                    { "@p_Org_ID", SqlDbType.Int },
                    { "@p_Default_Authentication_Policy_ID", SqlDbType.BigInt },
                    { "@p_Default_Authorization_Policy_ID", SqlDbType.BigInt },
                    { "@p_Default_SLA_Definition_ID", SqlDbType.BigInt },
                    { "@p_Default_Execution_Policy_ID", SqlDbType.BigInt },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int },
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_Org_Level_Default_Policy",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Domain to create or update a Workflow Domain record.
        /// </summary>
        public async Task<CommonDBResponse> SaveDomainAsync(SaveDomainRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveDomainAsync";
            var filterIds = new Dictionary<string, object> { { "DomainId", request.DomainId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_Domain_ID", request.DomainId ?? (object)DBNull.Value },
                    { "@p_Org_ID", request.OrgId },
                    { "@p_Parent_Domain_ID", request.ParentDomainId ?? (object)DBNull.Value },
                    { "@p_Domain_Code", request.DomainCode ?? (object)DBNull.Value },
                    { "@p_Domain_Name", request.DomainName ?? (object)DBNull.Value },
                    { "@p_Domain_Description", request.DomainDescription ?? (object)DBNull.Value },
                    { "@p_Domain_Type_Code", request.DomainTypeCode ?? (object)DBNull.Value },
                    { "@p_Domain_Owner_Team_Code", request.DomainOwnerTeamCode ?? (object)DBNull.Value },
                    { "@p_Default_Authentication_Policy_ID", request.DefaultAuthenticationPolicyId ?? (object)DBNull.Value },
                    { "@p_Default_Authorization_Policy_ID", request.DefaultAuthorizationPolicyId ?? (object)DBNull.Value },
                    { "@p_Default_SLA_Definition_ID", request.DefaultSlaDefinitionId ?? (object)DBNull.Value },
                    { "@p_Default_Execution_Policy_ID", request.DefaultExecutionPolicyId ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId }
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_Domain_ID", SqlDbType.BigInt },
                    { "@p_Org_ID", SqlDbType.Int },
                    { "@p_Parent_Domain_ID", SqlDbType.BigInt },
                    { "@p_Domain_Code", SqlDbType.NVarChar },
                    { "@p_Domain_Name", SqlDbType.NVarChar },
                    { "@p_Domain_Description", SqlDbType.NVarChar },
                    { "@p_Domain_Type_Code", SqlDbType.NVarChar },
                    { "@p_Domain_Owner_Team_Code", SqlDbType.NVarChar },
                    { "@p_Default_Authentication_Policy_ID", SqlDbType.BigInt },
                    { "@p_Default_Authorization_Policy_ID", SqlDbType.BigInt },
                    { "@p_Default_SLA_Definition_ID", SqlDbType.BigInt },
                    { "@p_Default_Execution_Policy_ID", SqlDbType.BigInt },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int }
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_Domain",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_SLA to create or update a Sla record.
        /// </summary>
        public async Task<CommonDBResponse> SaveSlaAsync(SaveSlaRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveSlaAsync";
            var filterIds = new Dictionary<string, object> { { "SlaId", request.SlaId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_SLA_ID", request.SlaId ?? (object)DBNull.Value },
                    { "@p_Parent_SLA_Definition_ID", request.ParentSlaDefinitionId ?? (object)DBNull.Value },
                    { "@p_Org_ID", request.OrgId },
                    { "@p_Domain_ID", request.DomainId ?? (object)DBNull.Value },
                    { "@p_SLA_Code", request.SlaCode ?? (object)DBNull.Value },
                    { "@p_SLA_Name", request.SlaName ?? (object)DBNull.Value },
                    { "@p_SLA_Description", request.SlaDescription ?? (object)DBNull.Value },
                    { "@p_SLA_Type_Code", request.SlaTypeCode ?? (object)DBNull.Value },
                    { "@p_SLA_Priority_Code", request.SlaPriorityCode ?? (object)DBNull.Value },
                    { "@p_Time_Calculation_Mode_Code", request.TimeCalculationModeCode ?? (object)DBNull.Value },
                    { "@p_Business_Calendar_Reference", request.BusinessCalendarReference ?? (object)DBNull.Value },
                    { "@p_Target_Duration_Seconds", request.TargetDurationSeconds ?? (object)DBNull.Value },
                    { "@p_Warning_Duration_Seconds", request.WarningDurationSeconds ?? (object)DBNull.Value },
                    { "@p_Breach_Duration_Seconds", request.BreachDurationSeconds ?? (object)DBNull.Value },
                    { "@p_Escalation_After_Seconds", request.EscalationAfterSeconds ?? (object)DBNull.Value },
                    { "@p_Escalation_Policy_Reference", request.EscalationPolicyReference ?? (object)DBNull.Value },
                    { "@p_Warning_Action_Code", request.WarningActionCode ?? (object)DBNull.Value },
                    { "@p_Breach_Action_Code", request.BreachActionCode ?? (object)DBNull.Value },
                    { "@p_Escalation_Action_Code", request.EscalationActionCode ?? (object)DBNull.Value },
                    { "@p_Pause_Condition_Expression", request.PauseConditionExpression ?? (object)DBNull.Value },
                    { "@p_Resume_Condition_Expression", request.ResumeConditionExpression ?? (object)DBNull.Value },
                    { "@p_SLA_Owner_Type_Code", request.SlaOwnerTypeCode ?? (object)DBNull.Value },
                    { "@p_SLA_Owner_Reference", request.SlaOwnerReference ?? (object)DBNull.Value },
                    { "@p_Is_Breach_Blocking_Flag", request.IsBreachBlockingFlag ?? (object)DBNull.Value },
                    { "@p_Is_Notification_Required_Flag", request.IsNotificationRequiredFlag ?? (object)DBNull.Value },
                    { "@p_Is_Reusable_Flag", request.IsReusableFlag ?? (object)DBNull.Value },
                    { "@p_Is_System_SLA_Flag", request.IsSystemSlaFlag ?? (object)DBNull.Value },
                    { "@p_Valid_From_Date", request.ValidFromDate ?? (object)DBNull.Value },
                    { "@p_Valid_To_Date", request.ValidToDate ?? (object)DBNull.Value },
                    { "@p_SLA_CONFIG_Json", request.SlaConfigJson ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId },
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_SLA_ID", SqlDbType.BigInt },
                    { "@p_Parent_SLA_Definition_ID", SqlDbType.BigInt },
                    { "@p_Org_ID", SqlDbType.Int },
                    { "@p_Domain_ID", SqlDbType.BigInt },
                    { "@p_SLA_Code", SqlDbType.NVarChar },
                    { "@p_SLA_Name", SqlDbType.NVarChar },
                    { "@p_SLA_Description", SqlDbType.NVarChar },
                    { "@p_SLA_Type_Code", SqlDbType.NVarChar },
                    { "@p_SLA_Priority_Code", SqlDbType.NVarChar },
                    { "@p_Time_Calculation_Mode_Code", SqlDbType.NVarChar },
                    { "@p_Business_Calendar_Reference", SqlDbType.NVarChar },
                    { "@p_Target_Duration_Seconds", SqlDbType.Int },
                    { "@p_Warning_Duration_Seconds", SqlDbType.Int },
                    { "@p_Breach_Duration_Seconds", SqlDbType.Int },
                    { "@p_Escalation_After_Seconds", SqlDbType.Int },
                    { "@p_Escalation_Policy_Reference", SqlDbType.NVarChar },
                    { "@p_Warning_Action_Code", SqlDbType.NVarChar },
                    { "@p_Breach_Action_Code", SqlDbType.NVarChar },
                    { "@p_Escalation_Action_Code", SqlDbType.NVarChar },
                    { "@p_Pause_Condition_Expression", SqlDbType.NVarChar },
                    { "@p_Resume_Condition_Expression", SqlDbType.NVarChar },
                    { "@p_SLA_Owner_Type_Code", SqlDbType.NVarChar },
                    { "@p_SLA_Owner_Reference", SqlDbType.NVarChar },
                    { "@p_Is_Breach_Blocking_Flag", SqlDbType.Bit },
                    { "@p_Is_Notification_Required_Flag", SqlDbType.Bit },
                    { "@p_Is_Reusable_Flag", SqlDbType.Bit },
                    { "@p_Is_System_SLA_Flag", SqlDbType.Bit },
                    { "@p_Valid_From_Date", SqlDbType.DateTime },
                    { "@p_Valid_To_Date", SqlDbType.DateTime },
                    { "@p_SLA_CONFIG_Json", SqlDbType.NVarChar },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int },
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_SLA",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Stage to create or update a Stage record.
        /// </summary>
        public async Task<CommonDBResponse> SaveStageAsync(SaveStageRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveStageAsync";
            var filterIds = new Dictionary<string, object> { { "StageId", request.StageId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_Stage_ID", request.StageId ?? (object)DBNull.Value },
                    { "@p_Parent_Stage_ID", request.ParentStageId ?? (object)DBNull.Value },
                    { "@p_Org_ID", request.OrgId },
                    { "@p_Domain_ID", request.DomainId ?? (object)DBNull.Value },
                    { "@p_Stage_Code", request.StageCode ?? (object)DBNull.Value },
                    { "@p_Stage_Name", request.StageName ?? (object)DBNull.Value },
                    { "@p_Stage_Description", request.StageDescription ?? (object)DBNull.Value },
                    { "@p_Authentication_Policy_ID", request.AuthenticationPolicyId ?? (object)DBNull.Value },
                    { "@p_Authorization_Policy_ID", request.AuthorizationPolicyId ?? (object)DBNull.Value },
                    { "@p_Rule_Reference_Type_Code", request.RuleReferenceTypeCode ?? (object)DBNull.Value },
                    { "@p_Rule_Reference_ID", request.RuleReferenceId ?? (object)DBNull.Value },
                    { "@p_Event_ID", request.EventId ?? (object)DBNull.Value },
                    { "@p_SLA_ID", request.SlaId ?? (object)DBNull.Value },
                    { "@p_Execution_Policy_ID", request.ExecutionPolicyId ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId },
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_Stage_ID", SqlDbType.BigInt },
                    { "@p_Parent_Stage_ID", SqlDbType.BigInt },
                    { "@p_Org_ID", SqlDbType.Int },
                    { "@p_Domain_ID", SqlDbType.BigInt },
                    { "@p_Stage_Code", SqlDbType.NVarChar },
                    { "@p_Stage_Name", SqlDbType.NVarChar },
                    { "@p_Stage_Description", SqlDbType.NVarChar },
                    { "@p_Authentication_Policy_ID", SqlDbType.BigInt },
                    { "@p_Authorization_Policy_ID", SqlDbType.BigInt },
                    { "@p_Rule_Reference_Type_Code", SqlDbType.NVarChar },
                    { "@p_Rule_Reference_ID", SqlDbType.BigInt },
                    { "@p_Event_ID", SqlDbType.BigInt },
                    { "@p_SLA_ID", SqlDbType.BigInt },
                    { "@p_Execution_Policy_ID", SqlDbType.BigInt },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int },
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_Stage",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Step to create or update a Step record.
        /// </summary>
        public async Task<CommonDBResponse> SaveStepAsync(SaveStepRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveStepAsync";
            var filterIds = new Dictionary<string, object> { { "StepId", request.StepId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_Step_ID", request.StepId ?? (object)DBNull.Value },
                    { "@p_Parent_Step_ID", request.ParentStepId ?? (object)DBNull.Value },
                    { "@p_Org_ID", request.OrgId },
                    { "@p_Domain_ID", request.DomainId ?? (object)DBNull.Value },
                    { "@p_Step_Code", request.StepCode ?? (object)DBNull.Value },
                    { "@p_Step_Name", request.StepName ?? (object)DBNull.Value },
                    { "@p_Step_Description", request.StepDescription ?? (object)DBNull.Value },
                    { "@p_Authentication_Policy_ID", request.AuthenticationPolicyId ?? (object)DBNull.Value },
                    { "@p_Authorization_Policy_ID", request.AuthorizationPolicyId ?? (object)DBNull.Value },
                    { "@p_Rule_Reference_Type_Code", request.RuleReferenceTypeCode ?? (object)DBNull.Value },
                    { "@p_Rule_Reference_ID", request.RuleReferenceId ?? (object)DBNull.Value },
                    { "@p_Event_ID", request.EventId ?? (object)DBNull.Value },
                    { "@p_SLA_ID", request.SlaId ?? (object)DBNull.Value },
                    { "@p_Execution_Policy_ID", request.ExecutionPolicyId ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId },
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_Step_ID", SqlDbType.BigInt },
                    { "@p_Parent_Step_ID", SqlDbType.BigInt },
                    { "@p_Org_ID", SqlDbType.Int },
                    { "@p_Domain_ID", SqlDbType.BigInt },
                    { "@p_Step_Code", SqlDbType.NVarChar },
                    { "@p_Step_Name", SqlDbType.NVarChar },
                    { "@p_Step_Description", SqlDbType.NVarChar },
                    { "@p_Authentication_Policy_ID", SqlDbType.BigInt },
                    { "@p_Authorization_Policy_ID", SqlDbType.BigInt },
                    { "@p_Rule_Reference_Type_Code", SqlDbType.NVarChar },
                    { "@p_Rule_Reference_ID", SqlDbType.BigInt },
                    { "@p_Event_ID", SqlDbType.BigInt },
                    { "@p_SLA_ID", SqlDbType.BigInt },
                    { "@p_Execution_Policy_ID", SqlDbType.BigInt },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int },
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_Step",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Task to create or update a TaskDefinition record.
        /// </summary>
        public async Task<CommonDBResponse> SaveTaskAsync(SaveTaskRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveTaskAsync";
            var filterIds = new Dictionary<string, object> { { "TaskId", request.TaskId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_Task_ID", request.TaskId ?? (object)DBNull.Value },
                    { "@p_Parent_Task_ID", request.ParentTaskId ?? (object)DBNull.Value },
                    { "@p_Org_ID", request.OrgId },
                    { "@p_Domain_ID", request.DomainId ?? (object)DBNull.Value },
                    { "@p_Task_Code", request.TaskCode ?? (object)DBNull.Value },
                    { "@p_Task_Name", request.TaskName ?? (object)DBNull.Value },
                    { "@p_Task_Description", request.TaskDescription ?? (object)DBNull.Value },
                    { "@p_Task_Type_Code", request.TaskTypeCode ?? (object)DBNull.Value },
                    { "@p_Task_Level_Code", request.TaskLevelCode ?? (object)DBNull.Value },
                    { "@p_Task_Execution_Mode_Code", request.TaskExecutionModeCode ?? (object)DBNull.Value },
                    { "@p_Task_Owner_Type_Code", request.TaskOwnerTypeCode ?? (object)DBNull.Value },
                    { "@p_Task_Owner_Reference", request.TaskOwnerReference ?? (object)DBNull.Value },
                    { "@p_Authentication_Policy_ID", request.AuthenticationPolicyId ?? (object)DBNull.Value },
                    { "@p_Authorization_Policy_ID", request.AuthorizationPolicyId ?? (object)DBNull.Value },
                    { "@p_Rule_Reference_Type_Code", request.RuleReferenceTypeCode ?? (object)DBNull.Value },
                    { "@p_Rule_Reference_ID", request.RuleReferenceId ?? (object)DBNull.Value },
                    { "@p_Event_ID", request.EventId ?? (object)DBNull.Value },
                    { "@p_SLA_ID", request.SlaId ?? (object)DBNull.Value },
                    { "@p_Execution_Policy_ID", request.ExecutionPolicyId ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId },
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_Task_ID", SqlDbType.BigInt },
                    { "@p_Parent_Task_ID", SqlDbType.BigInt },
                    { "@p_Org_ID", SqlDbType.Int },
                    { "@p_Domain_ID", SqlDbType.BigInt },
                    { "@p_Task_Code", SqlDbType.NVarChar },
                    { "@p_Task_Name", SqlDbType.NVarChar },
                    { "@p_Task_Description", SqlDbType.NVarChar },
                    { "@p_Task_Type_Code", SqlDbType.NVarChar },
                    { "@p_Task_Level_Code", SqlDbType.NVarChar },
                    { "@p_Task_Execution_Mode_Code", SqlDbType.NVarChar },
                    { "@p_Task_Owner_Type_Code", SqlDbType.NVarChar },
                    { "@p_Task_Owner_Reference", SqlDbType.NVarChar },
                    { "@p_Authentication_Policy_ID", SqlDbType.BigInt },
                    { "@p_Authorization_Policy_ID", SqlDbType.BigInt },
                    { "@p_Rule_Reference_Type_Code", SqlDbType.NVarChar },
                    { "@p_Rule_Reference_ID", SqlDbType.BigInt },
                    { "@p_Event_ID", SqlDbType.BigInt },
                    { "@p_SLA_ID", SqlDbType.BigInt },
                    { "@p_Execution_Policy_ID", SqlDbType.BigInt },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int },
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_Task",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Action to create or update a ActionDefinition record.
        /// </summary>
        public async Task<CommonDBResponse> SaveActionAsync(SaveActionRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveActionAsync";
            var filterIds = new Dictionary<string, object> { { "ActionDefinitionId", request.ActionDefinitionId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_Action_Definition_ID", request.ActionDefinitionId ?? (object)DBNull.Value },
                    { "@p_Org_ID", request.OrgId },
                    { "@p_Domain_ID", request.DomainId ?? (object)DBNull.Value },
                    { "@p_Action_Code", request.ActionCode ?? (object)DBNull.Value },
                    { "@p_Action_Name", request.ActionName ?? (object)DBNull.Value },
                    { "@p_Action_Description", request.ActionDescription ?? (object)DBNull.Value },
                    { "@p_Action_Type_Code", request.ActionTypeCode ?? (object)DBNull.Value },
                    { "@p_Action_Category_Code", request.ActionCategoryCode ?? (object)DBNull.Value },
                    { "@p_Action_Handler_Reference", request.ActionHandlerReference ?? (object)DBNull.Value },
                    { "@p_Action_Input_Schema_Reference", request.ActionInputSchemaReference ?? (object)DBNull.Value },
                    { "@p_Action_Output_Schema_Reference", request.ActionOutputSchemaReference ?? (object)DBNull.Value },
                    { "@p_Success_Condition_Expression", request.SuccessConditionExpression ?? (object)DBNull.Value },
                    { "@p_Failure_Condition_Expression", request.FailureConditionExpression ?? (object)DBNull.Value },
                    { "@p_Authentication_Policy_ID", request.AuthenticationPolicyId ?? (object)DBNull.Value },
                    { "@p_Authorization_Policy_ID", request.AuthorizationPolicyId ?? (object)DBNull.Value },
                    { "@p_Rule_Reference_Type_Code", request.RuleReferenceTypeCode ?? (object)DBNull.Value },
                    { "@p_Rule_Reference_ID", request.RuleReferenceId ?? (object)DBNull.Value },
                    { "@p_Event_ID", request.EventId ?? (object)DBNull.Value },
                    { "@p_SLA_ID", request.SlaId ?? (object)DBNull.Value },
                    { "@p_Execution_Policy_ID", request.ExecutionPolicyId ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId },
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_Action_Definition_ID", SqlDbType.BigInt },
                    { "@p_Org_ID", SqlDbType.Int },
                    { "@p_Domain_ID", SqlDbType.BigInt },
                    { "@p_Action_Code", SqlDbType.NVarChar },
                    { "@p_Action_Name", SqlDbType.NVarChar },
                    { "@p_Action_Description", SqlDbType.NVarChar },
                    { "@p_Action_Type_Code", SqlDbType.NVarChar },
                    { "@p_Action_Category_Code", SqlDbType.NVarChar },
                    { "@p_Action_Handler_Reference", SqlDbType.NVarChar },
                    { "@p_Action_Input_Schema_Reference", SqlDbType.NVarChar },
                    { "@p_Action_Output_Schema_Reference", SqlDbType.NVarChar },
                    { "@p_Success_Condition_Expression", SqlDbType.NVarChar },
                    { "@p_Failure_Condition_Expression", SqlDbType.NVarChar },
                    { "@p_Authentication_Policy_ID", SqlDbType.BigInt },
                    { "@p_Authorization_Policy_ID", SqlDbType.BigInt },
                    { "@p_Rule_Reference_Type_Code", SqlDbType.NVarChar },
                    { "@p_Rule_Reference_ID", SqlDbType.BigInt },
                    { "@p_Event_ID", SqlDbType.BigInt },
                    { "@p_SLA_ID", SqlDbType.BigInt },
                    { "@p_Execution_Policy_ID", SqlDbType.BigInt },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int },
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_Action",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Entity to create or update a EntityDefinition record.
        /// </summary>
        public async Task<CommonDBResponse> SaveEntityAsync(SaveEntityRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveEntityAsync";
            var filterIds = new Dictionary<string, object> { { "EntityDefinitionId", request.EntityDefinitionId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_Entity_Definition_ID", request.EntityDefinitionId ?? (object)DBNull.Value },
                    { "@p_Parent_Entity_Definition_ID", request.ParentEntityDefinitionId ?? (object)DBNull.Value },
                    { "@p_Org_ID", request.OrgId },
                    { "@p_Domain_ID", request.DomainId ?? (object)DBNull.Value },
                    { "@p_Entity_Code", request.EntityCode ?? (object)DBNull.Value },
                    { "@p_Entity_Name", request.EntityName ?? (object)DBNull.Value },
                    { "@p_Entity_Description", request.EntityDescription ?? (object)DBNull.Value },
                    { "@p_Entity_Type_Code", request.EntityTypeCode ?? (object)DBNull.Value },
                    { "@p_Entity_Category_Code", request.EntityCategoryCode ?? (object)DBNull.Value },
                    { "@p_Entity_Source_Type_Code", request.EntitySourceTypeCode ?? (object)DBNull.Value },
                    { "@p_Source_System_Reference", request.SourceSystemReference ?? (object)DBNull.Value },
                    { "@p_Logical_Entity_Name", request.LogicalEntityName ?? (object)DBNull.Value },
                    { "@p_Physical_Entity_Reference", request.PhysicalEntityReference ?? (object)DBNull.Value },
                    { "@p_Primary_Key_Attribute_Name", request.PrimaryKeyAttributeName ?? (object)DBNull.Value },
                    { "@p_Display_Attribute_Name", request.DisplayAttributeName ?? (object)DBNull.Value },
                    { "@p_Entity_Schema_Reference", request.EntitySchemaReference ?? (object)DBNull.Value },
                    { "@p_Default_Context_Path", request.DefaultContextPath ?? (object)DBNull.Value },
                    { "@p_Is_Core_Entity_Flag", request.IsCoreEntityFlag ?? (object)DBNull.Value },
                    { "@p_Is_Reusable_Flag", request.IsReusableFlag ?? (object)DBNull.Value },
                    { "@p_Is_System_Entity_Flag", request.IsSystemEntityFlag ?? (object)DBNull.Value },
                    { "@p_Entity_CONFIG_Json", request.EntityConfigJson ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId },
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_Entity_Definition_ID", SqlDbType.BigInt },
                    { "@p_Parent_Entity_Definition_ID", SqlDbType.BigInt },
                    { "@p_Org_ID", SqlDbType.Int },
                    { "@p_Domain_ID", SqlDbType.BigInt },
                    { "@p_Entity_Code", SqlDbType.NVarChar },
                    { "@p_Entity_Name", SqlDbType.NVarChar },
                    { "@p_Entity_Description", SqlDbType.NVarChar },
                    { "@p_Entity_Type_Code", SqlDbType.NVarChar },
                    { "@p_Entity_Category_Code", SqlDbType.NVarChar },
                    { "@p_Entity_Source_Type_Code", SqlDbType.NVarChar },
                    { "@p_Source_System_Reference", SqlDbType.NVarChar },
                    { "@p_Logical_Entity_Name", SqlDbType.NVarChar },
                    { "@p_Physical_Entity_Reference", SqlDbType.NVarChar },
                    { "@p_Primary_Key_Attribute_Name", SqlDbType.NVarChar },
                    { "@p_Display_Attribute_Name", SqlDbType.NVarChar },
                    { "@p_Entity_Schema_Reference", SqlDbType.NVarChar },
                    { "@p_Default_Context_Path", SqlDbType.NVarChar },
                    { "@p_Is_Core_Entity_Flag", SqlDbType.Bit },
                    { "@p_Is_Reusable_Flag", SqlDbType.Bit },
                    { "@p_Is_System_Entity_Flag", SqlDbType.Bit },
                    { "@p_Entity_CONFIG_Json", SqlDbType.NVarChar },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int },
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_Entity",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Entity_Association to create or update a EntityAssociation record.
        /// </summary>
        public async Task<CommonDBResponse> SaveEntityAssociationAsync(SaveEntityAssociationRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveEntityAssociationAsync";
            var filterIds = new Dictionary<string, object> { { "WorkflowEntityAssociationId", request.WorkflowEntityAssociationId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId: correlationId, filterIds: filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId: correlationId, filterIds: filterIds, data: new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var inputParameters = new Dictionary<string, object>
                {
                    { "@p_Workflow_Entity_Association_ID", request.WorkflowEntityAssociationId ?? (object)DBNull.Value },
                    { "@p_Workflow_Version_ID", request.WorkflowVersionId ?? (object)DBNull.Value },
                    { "@p_Association_Level_Code", request.AssociationLevelCode ?? (object)DBNull.Value },
                    { "@p_Association_Reference_ID", request.AssociationReferenceId ?? (object)DBNull.Value },
                    { "@p_Entity_Definition_ID", request.EntityDefinitionId ?? (object)DBNull.Value },
                    { "@p_Entity_Alias", request.EntityAlias ?? (object)DBNull.Value },
                    { "@p_Entity_Role_Code", request.EntityRoleCode ?? (object)DBNull.Value },
                    { "@p_Entity_Usage_Code", request.EntityUsageCode ?? (object)DBNull.Value },
                    { "@p_Entity_Access_Mode_Code", request.EntityAccessModeCode ?? (object)DBNull.Value },
                    { "@p_Cardinality_Code", request.CardinalityCode ?? (object)DBNull.Value },
                    { "@p_Is_Primary_Entity_Flag", request.IsPrimaryEntityFlag ?? (object)DBNull.Value },
                    { "@p_Is_Required_Flag", request.IsRequiredFlag ?? (object)DBNull.Value },
                    { "@p_Is_Context_Entity_Flag", request.IsContextEntityFlag ?? (object)DBNull.Value },
                    { "@p_Context_Path", request.ContextPath ?? (object)DBNull.Value },
                    { "@p_Entity_Key_Context_Path", request.EntityKeyContextPath ?? (object)DBNull.Value },
                    { "@p_Related_Entity_Definition_ID", request.RelatedEntityDefinitionId ?? (object)DBNull.Value },
                    { "@p_Entity_Relationship_Type_Code", request.EntityRelationshipTypeCode ?? (object)DBNull.Value },
                    { "@p_Relationship_Expression", request.RelationshipExpression ?? (object)DBNull.Value },
                    { "@p_Filter_Expression", request.FilterExpression ?? (object)DBNull.Value },
                    { "@p_Validation_Rule_Set_ID", request.ValidationRuleSetId ?? (object)DBNull.Value },
                    { "@p_Sequence_Number", request.SequenceNumber ?? (object)DBNull.Value },
                    { "@p_Association_CONFIG_Json", request.AssociationConfigJson ?? (object)DBNull.Value },
                    { "@p_Status_Code", request.StatusCode ?? (object)DBNull.Value },
                    { "@p_User_ID", request.UserId },
                };

                var inputParameterTypes = new Dictionary<string, SqlDbType>
                {
                    { "@p_Workflow_Entity_Association_ID", SqlDbType.BigInt },
                    { "@p_Workflow_Version_ID", SqlDbType.BigInt },
                    { "@p_Association_Level_Code", SqlDbType.NVarChar },
                    { "@p_Association_Reference_ID", SqlDbType.BigInt },
                    { "@p_Entity_Definition_ID", SqlDbType.BigInt },
                    { "@p_Entity_Alias", SqlDbType.NVarChar },
                    { "@p_Entity_Role_Code", SqlDbType.NVarChar },
                    { "@p_Entity_Usage_Code", SqlDbType.NVarChar },
                    { "@p_Entity_Access_Mode_Code", SqlDbType.NVarChar },
                    { "@p_Cardinality_Code", SqlDbType.NVarChar },
                    { "@p_Is_Primary_Entity_Flag", SqlDbType.Bit },
                    { "@p_Is_Required_Flag", SqlDbType.Bit },
                    { "@p_Is_Context_Entity_Flag", SqlDbType.Bit },
                    { "@p_Context_Path", SqlDbType.NVarChar },
                    { "@p_Entity_Key_Context_Path", SqlDbType.NVarChar },
                    { "@p_Related_Entity_Definition_ID", SqlDbType.BigInt },
                    { "@p_Entity_Relationship_Type_Code", SqlDbType.NVarChar },
                    { "@p_Relationship_Expression", SqlDbType.NVarChar },
                    { "@p_Filter_Expression", SqlDbType.NVarChar },
                    { "@p_Validation_Rule_Set_ID", SqlDbType.BigInt },
                    { "@p_Sequence_Number", SqlDbType.Int },
                    { "@p_Association_CONFIG_Json", SqlDbType.NVarChar },
                    { "@p_Status_Code", SqlDbType.SmallInt },
                    { "@p_User_ID", SqlDbType.Int },
                };

                var outputParameters = new Dictionary<string, SqlDbType>
                {
                    { "@p_Output_Status_Code", SqlDbType.NVarChar },
                    { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                };

                var outputParameterSizes = new Dictionary<string, int>
                {
                    { "@p_Output_Status_Code", 1000 },
                    { "@p_Output_Status_Msg", 1000 }
                };

                var result = await _mssqlConnector.ExecuteStoredProcedureAsync(
                    procedureName: "sp_WF_CONFIG_Save_Entity_Association",
                    inputParameters: inputParameters,
                    inputParameterTypes: inputParameterTypes,
                    outputParameters: outputParameters,
                    outputParameterSizes: outputParameterSizes,
                    serviceName: serviceName,
                    correlationId: correlationId,
                    customSchemaName: Schema);

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters?["@p_Output_Status_Code"]?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters?["@p_Output_Status_Msg"]?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId: correlationId, filterIds: filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    errorCode: "DB_EXCEPTION",
                    errorDetails: ex.Message,
                    correlationId: correlationId,
                    filterIds: filterIds,
                    exception: ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Rule to create or update a Rule record, including
        /// its table-valued list of Rule_CONFIG key/value settings.
        /// </summary>
        public async Task<CommonDBResponse> SaveRuleAsync(SaveRuleRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveRuleAsync";
            var filterIds = new Dictionary<string, object> { { "RuleId", request.RuleId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId, filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId, filterIds, new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var ruleConfigTable = BuildRuleConfigDataTable(request.RuleConfigList);

                var spRequest = new SpExecutionRequest
                {
                    ProcedureName = "sp_WF_CONFIG_Save_Rule",
                    ServiceName = serviceName,
                    CorrelationId = correlationId,
                    InputParameters = new Dictionary<string, object?>
                    {
                        { "@p_Rule_ID", request.RuleId },
                        { "@p_Rule_Set_ID", request.RuleSetId },
                        { "@p_Parent_Rule_ID", request.ParentRuleId },
                        { "@p_Org_ID", request.OrgId },
                        { "@p_Domain_ID", request.DomainId },
                        { "@p_Rule_Code", request.RuleCode },
                        { "@p_Rule_Name", request.RuleName },
                        { "@p_Rule_Description", request.RuleDescription },
                        { "@p_Rule_Type_Code", request.RuleTypeCode },
                        { "@p_Rule_Category_Code", request.RuleCategoryCode },
                        { "@p_Evaluation_Type_Code", request.EvaluationTypeCode },
                        { "@p_Evaluator_Reference", request.EvaluatorReference },
                        { "@p_Condition_Expression", request.ConditionExpression },
                        { "@p_Input_Context_Path", request.InputContextPath },
                        { "@p_Output_Context_Path", request.OutputContextPath },
                        { "@p_Success_Outcome_Code", request.SuccessOutcomeCode },
                        { "@p_Failure_Outcome_Code", request.FailureOutcomeCode },
                        { "@p_Default_Outcome_Code", request.DefaultOutcomeCode },
                        { "@p_Failure_Handling_Mode_Code", request.FailureHandlingModeCode },
                        { "@p_Priority_Number", request.PriorityNumber },
                        { "@p_Severity_Code", request.SeverityCode },
                        { "@p_Is_Blocking_Rule_Flag", request.IsBlockingRuleFlag },
                        { "@p_Is_Reusable_Flag", request.IsReusableFlag },
                        { "@p_Is_System_Rule_Flag", request.IsSystemRuleFlag },
                        { "@p_Valid_From_Date", request.ValidFromDate },
                        { "@p_Valid_To_Date", request.ValidToDate },
                        { "@p_Rule_CONFIG_Json", request.RuleConfigJson },
                        { "@p_Status_Code", request.StatusCode },
                        { "@p_User_ID", request.UserId }
                    },
                    InputParameterTypes = new Dictionary<string, SqlDbType>
                    {
                        { "@p_Rule_ID", SqlDbType.BigInt },
                        { "@p_Rule_Set_ID", SqlDbType.BigInt },
                        { "@p_Parent_Rule_ID", SqlDbType.BigInt },
                        { "@p_Org_ID", SqlDbType.Int },
                        { "@p_Domain_ID", SqlDbType.BigInt },
                        { "@p_Rule_Code", SqlDbType.NVarChar },
                        { "@p_Rule_Name", SqlDbType.NVarChar },
                        { "@p_Rule_Description", SqlDbType.NVarChar },
                        { "@p_Rule_Type_Code", SqlDbType.NVarChar },
                        { "@p_Rule_Category_Code", SqlDbType.NVarChar },
                        { "@p_Evaluation_Type_Code", SqlDbType.NVarChar },
                        { "@p_Evaluator_Reference", SqlDbType.NVarChar },
                        { "@p_Condition_Expression", SqlDbType.NVarChar },
                        { "@p_Input_Context_Path", SqlDbType.NVarChar },
                        { "@p_Output_Context_Path", SqlDbType.NVarChar },
                        { "@p_Success_Outcome_Code", SqlDbType.NVarChar },
                        { "@p_Failure_Outcome_Code", SqlDbType.NVarChar },
                        { "@p_Default_Outcome_Code", SqlDbType.NVarChar },
                        { "@p_Failure_Handling_Mode_Code", SqlDbType.NVarChar },
                        { "@p_Priority_Number", SqlDbType.Int },
                        { "@p_Severity_Code", SqlDbType.NVarChar },
                        { "@p_Is_Blocking_Rule_Flag", SqlDbType.Bit },
                        { "@p_Is_Reusable_Flag", SqlDbType.Bit },
                        { "@p_Is_System_Rule_Flag", SqlDbType.Bit },
                        { "@p_Valid_From_Date", SqlDbType.DateTime },
                        { "@p_Valid_To_Date", SqlDbType.DateTime },
                        { "@p_Rule_CONFIG_Json", SqlDbType.NVarChar },
                        { "@p_Status_Code", SqlDbType.SmallInt },
                        { "@p_User_ID", SqlDbType.Int }
                    },
                    TvpParameters = new Dictionary<string, TvpParameter>
                    {
                        {
                            "@p_t_WF_CONFIG_Rule_CONFIG_List",
                            new TvpParameter
                            {
                                TypeName = "Workflow.t_WF_CONFIG_Rule_CONFIG",
                                Data = ruleConfigTable
                            }
                        }
                    },
                    OutputParameters = new Dictionary<string, SqlDbType>
                    {
                        { "@p_Output_Status_Code", SqlDbType.NVarChar },
                        { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                    },
                    OutputParameterSizes = new Dictionary<string, int>
                    {
                        { "@p_Output_Status_Code", 1000 },
                        { "@p_Output_Status_Msg", 1000 }
                    }
                };

                var result = await _sqlServerConnector2.ExecuteAsync(spRequest, CancellationToken.None);

                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed to execute sp_WF_CONFIG_Save_Rule: {result.ErrorCode} - {result.ErrorMessage}");
                }

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters.GetValueOrDefault("@p_Output_Status_Code")?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters.GetValueOrDefault("@p_Output_Status_Msg")?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId, filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    "DB_EXCEPTION",
                    "RepositoryException",
                    ex.Message,
                    correlationId,
                    filterIds,
                    ex);
                throw;
            }
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Authorization_Policy to create or update an
        /// Authorization Policy record, including its table-valued list of
        /// Authorization Policy Rules.
        /// </summary>
        public async Task<CommonDBResponse> SaveAuthorizationPolicyAsync(SaveAuthorizationPolicyRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveAuthorizationPolicyAsync";
            var filterIds = new Dictionary<string, object> { { "AuthorizationPolicyId", request.AuthorizationPolicyId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId, filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId, filterIds, new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var ruleTable = BuildAuthorizationPolicyRuleDataTable(request.AuthorizationPolicyRuleList);

                var spRequest = new SpExecutionRequest
                {
                    ProcedureName = "sp_WF_CONFIG_Save_Authorization_Policy",
                    ServiceName = serviceName,
                    CorrelationId = correlationId,
                    InputParameters = new Dictionary<string, object?>
                    {
                        { "@p_Authorization_Policy_ID", request.AuthorizationPolicyId },
                        { "@p_Org_ID", request.OrgId },
                        { "@p_Domain_ID", request.DomainId },
                        { "@p_Authorization_Policy_Code", request.AuthorizationPolicyCode },
                        { "@p_Authorization_Policy_Name", request.AuthorizationPolicyName },
                        { "@p_Authorization_Policy_Description", request.AuthorizationPolicyDescription },
                        { "@p_Authorization_Policy_Type_Code", request.AuthorizationPolicyTypeCode },
                        { "@p_Authorization_Scope_Code", request.AuthorizationScopeCode },
                        { "@p_Evaluation_Mode_Code", request.EvaluationModeCode },
                        { "@p_Default_Decision_Code", request.DefaultDecisionCode },
                        { "@p_Priority_Number", request.PriorityNumber },
                        { "@p_External_Policy_Reference", request.ExternalPolicyReference },
                        { "@p_Policy_Expression", request.PolicyExpression },
                        { "@p_Is_External_Authorization_Flag", request.IsExternalAuthorizationFlag },
                        { "@p_Is_System_Policy_Flag", request.IsSystemPolicyFlag },
                        { "@p_Policy_CONFIG_Json", request.PolicyConfigJson },
                        { "@p_Status_Code", request.StatusCode },
                        { "@p_User_ID", request.UserId }
                    },
                    InputParameterTypes = new Dictionary<string, SqlDbType>
                    {
                        { "@p_Authorization_Policy_ID", SqlDbType.BigInt },
                        { "@p_Org_ID", SqlDbType.Int },
                        { "@p_Domain_ID", SqlDbType.BigInt },
                        { "@p_Authorization_Policy_Code", SqlDbType.NVarChar },
                        { "@p_Authorization_Policy_Name", SqlDbType.NVarChar },
                        { "@p_Authorization_Policy_Description", SqlDbType.NVarChar },
                        { "@p_Authorization_Policy_Type_Code", SqlDbType.NVarChar },
                        { "@p_Authorization_Scope_Code", SqlDbType.NVarChar },
                        { "@p_Evaluation_Mode_Code", SqlDbType.NVarChar },
                        { "@p_Default_Decision_Code", SqlDbType.NVarChar },
                        { "@p_Priority_Number", SqlDbType.Int },
                        { "@p_External_Policy_Reference", SqlDbType.NVarChar },
                        { "@p_Policy_Expression", SqlDbType.NVarChar },
                        { "@p_Is_External_Authorization_Flag", SqlDbType.Bit },
                        { "@p_Is_System_Policy_Flag", SqlDbType.Bit },
                        { "@p_Policy_CONFIG_Json", SqlDbType.NVarChar },
                        { "@p_Status_Code", SqlDbType.SmallInt },
                        { "@p_User_ID", SqlDbType.Int }
                    },
                    TvpParameters = new Dictionary<string, TvpParameter>
                    {
                        {
                            "@p_t_WF_CONFIG_Authorization_Policy_Rule_List",
                            new TvpParameter
                            {
                                TypeName = "Workflow.t_WF_CONFIG_Authorization_Policy_Rule",
                                Data = ruleTable
                            }
                        }
                    },
                    OutputParameters = new Dictionary<string, SqlDbType>
                    {
                        { "@p_Output_Status_Code", SqlDbType.NVarChar },
                        { "@p_Output_Status_Msg", SqlDbType.NVarChar }
                    },
                    OutputParameterSizes = new Dictionary<string, int>
                    {
                        { "@p_Output_Status_Code", 1000 },
                        { "@p_Output_Status_Msg", 1000 }
                    }
                };

                var result = await _sqlServerConnector2.ExecuteAsync(spRequest, CancellationToken.None);

                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed to execute sp_WF_CONFIG_Save_Authorization_Policy: {result.ErrorCode} - {result.ErrorMessage}");
                }

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters.GetValueOrDefault("@p_Output_Status_Code")?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters.GetValueOrDefault("@p_Output_Status_Msg")?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId, filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    "DB_EXCEPTION",
                    "RepositoryException",
                    ex.Message,
                    correlationId,
                    filterIds,
                    ex);
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Private helpers - DataTable builders for table-valued parameters
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Builds the DataTable matching [Workflow].[t_WF_CONFIG_Rule_CONFIG], in the
        /// exact column order defined by the SQL table type.
        /// </summary>
        private static DataTable BuildRuleConfigDataTable(List<RuleConfigItem> items)
        {
            var table = new DataTable();
            table.Columns.Add("Rule_CONFIG_ID", typeof(long));
            table.Columns.Add("Config_Key", typeof(string));
            table.Columns.Add("Config_Value", typeof(string));
            table.Columns.Add("Config_Data_Type_Code", typeof(short));
            table.Columns.Add("Config_Value_Unit_Code", typeof(short));
            table.Columns.Add("Config_Category_Code", typeof(short));
            table.Columns.Add("Config_Description", typeof(string));
            table.Columns.Add("Is_Required_Flag", typeof(bool));
            table.Columns.Add("Is_Sensitive_Flag", typeof(bool));
            table.Columns.Add("Display_Order", typeof(int));
            table.Columns.Add("Validation_Expression", typeof(string));
            table.Columns.Add("Default_Value", typeof(string));
            table.Columns.Add("Allowed_Values", typeof(string));
            table.Columns.Add("Status_Code", typeof(short));

            if (items == null)
            {
                return table;
            }

            foreach (var item in items)
            {
                var row = table.NewRow();
                row["Rule_CONFIG_ID"] = item.RuleConfigId ?? (object)DBNull.Value;
                row["Config_Key"] = item.ConfigKey ?? (object)DBNull.Value;
                row["Config_Value"] = item.ConfigValue ?? (object)DBNull.Value;
                row["Config_Data_Type_Code"] = item.ConfigDataTypeCode ?? (object)DBNull.Value;
                row["Config_Value_Unit_Code"] = item.ConfigValueUnitCode ?? (object)DBNull.Value;
                row["Config_Category_Code"] = item.ConfigCategoryCode ?? (object)DBNull.Value;
                row["Config_Description"] = item.ConfigDescription ?? (object)DBNull.Value;
                row["Is_Required_Flag"] = item.IsRequiredFlag ?? (object)DBNull.Value;
                row["Is_Sensitive_Flag"] = item.IsSensitiveFlag ?? (object)DBNull.Value;
                row["Display_Order"] = item.DisplayOrder ?? (object)DBNull.Value;
                row["Validation_Expression"] = item.ValidationExpression ?? (object)DBNull.Value;
                row["Default_Value"] = item.DefaultValue ?? (object)DBNull.Value;
                row["Allowed_Values"] = item.AllowedValues ?? (object)DBNull.Value;
                row["Status_Code"] = item.StatusCode ?? (object)DBNull.Value;
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Builds the DataTable matching [Workflow].[t_WF_CONFIG_Authorization_Policy_Rule],
        /// in the exact column order defined by the SQL table type.
        /// </summary>
        private static DataTable BuildAuthorizationPolicyRuleDataTable(List<AuthorizationPolicyRuleItem> items)
        {
            var table = new DataTable();
            table.Columns.Add("Authorization_Policy_Rule_ID", typeof(long));
            table.Columns.Add("Authorization_Rule_Code", typeof(string));
            table.Columns.Add("Authorization_Rule_Name", typeof(string));
            table.Columns.Add("Authorization_Rule_Description", typeof(string));
            table.Columns.Add("Principal_Type_Code", typeof(short));
            table.Columns.Add("Principal_Reference", typeof(string));
            table.Columns.Add("Resource_Type_Code", typeof(short));
            table.Columns.Add("Resource_Reference", typeof(string));
            table.Columns.Add("Action_Code", typeof(short));
            table.Columns.Add("Permission_Effect_Code", typeof(short));
            table.Columns.Add("Condition_Expression", typeof(string));
            table.Columns.Add("Evaluation_Priority_Number", typeof(int));
            table.Columns.Add("Is_Default_Rule_Flag", typeof(bool));
            table.Columns.Add("Valid_From_Date", typeof(DateTime));
            table.Columns.Add("Valid_To_Date", typeof(DateTime));
            table.Columns.Add("External_Rule_Reference", typeof(string));
            table.Columns.Add("Rule_CONFIG_Json", typeof(string));
            table.Columns.Add("Status_Code", typeof(short));

            if (items == null)
            {
                return table;
            }

            foreach (var item in items)
            {
                var row = table.NewRow();
                row["Authorization_Policy_Rule_ID"] = item.AuthorizationPolicyRuleId ?? (object)DBNull.Value;
                row["Authorization_Rule_Code"] = item.AuthorizationRuleCode ?? (object)DBNull.Value;
                row["Authorization_Rule_Name"] = item.AuthorizationRuleName ?? (object)DBNull.Value;
                row["Authorization_Rule_Description"] = item.AuthorizationRuleDescription ?? (object)DBNull.Value;
                row["Principal_Type_Code"] = item.PrincipalTypeCode ?? (object)DBNull.Value;
                row["Principal_Reference"] = item.PrincipalReference ?? (object)DBNull.Value;
                row["Resource_Type_Code"] = item.ResourceTypeCode ?? (object)DBNull.Value;
                row["Resource_Reference"] = item.ResourceReference ?? (object)DBNull.Value;
                row["Action_Code"] = item.ActionCode ?? (object)DBNull.Value;
                row["Permission_Effect_Code"] = item.PermissionEffectCode ?? (object)DBNull.Value;
                row["Condition_Expression"] = item.ConditionExpression ?? (object)DBNull.Value;
                row["Evaluation_Priority_Number"] = item.EvaluationPriorityNumber ?? (object)DBNull.Value;
                row["Is_Default_Rule_Flag"] = item.IsDefaultRuleFlag ?? (object)DBNull.Value;
                row["Valid_From_Date"] = item.ValidFromDate ?? (object)DBNull.Value;
                row["Valid_To_Date"] = item.ValidToDate ?? (object)DBNull.Value;
                row["External_Rule_Reference"] = item.ExternalRuleReference ?? (object)DBNull.Value;
                row["Rule_CONFIG_Json"] = item.RuleConfigJson ?? (object)DBNull.Value;
                row["Status_Code"] = item.StatusCode ?? (object)DBNull.Value;
                table.Rows.Add(row);
            }

            return table;
        }

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Event to create or update a Workflow Event
        /// definition, including its table-valued list of Event Actions.
        /// </summary>
        public async Task<CommonDBResponse> SaveEventAsync(SaveEventRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveEventAsync";
            var filterIds = new Dictionary<string, object> { { "EventId", request.EventId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId, filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId, filterIds, new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var eventActionTable = BuildEventActionDataTable(request.EventActionList);

                var spRequest = new SpExecutionRequest
                {
                    ProcedureName = "sp_WF_CONFIG_Save_Event",
                    ServiceName = serviceName,
                    CorrelationId = correlationId,
                    InputParameters = new Dictionary<string, object?>
            {
                { "@p_Event_ID",                          request.EventId },
                { "@p_Parent_Event_Definition_ID",        request.ParentEventDefinitionId },
                { "@p_Org_ID",                            request.OrgId },
                { "@p_Domain_ID",                         request.DomainId },
                { "@p_Event_Code",                        request.EventCode },
                { "@p_Event_Name",                        request.EventName },
                { "@p_Event_Description",                 request.EventDescription },
                { "@p_Event_Type_Code",                   request.EventTypeCode },
                { "@p_Event_Category_Code",               request.EventCategoryCode },
                { "@p_Event_Source_Type_Code",            request.EventSourceTypeCode },
                { "@p_Event_Source_Reference",            request.EventSourceReference },
                { "@p_Event_Payload_Schema_Reference",    request.EventPayloadSchemaReference },
                { "@p_Event_Correlation_Key_Expression",  request.EventCorrelationKeyExpression },
                { "@p_Event_Filter_Expression",           request.EventFilterExpression },
                { "@p_Rule_Set_ID",                       request.RuleSetId },
                { "@p_Timeout_Duration_Seconds",          request.TimeoutDurationSeconds },
                { "@p_Timeout_Action_Code",               request.TimeoutActionCode },
                { "@p_Is_Replay_Allowed_Flag",            request.IsReplayAllowedFlag },
                { "@p_Is_Duplicate_Allowed_Flag",         request.IsDuplicateAllowedFlag },
                { "@p_Is_Event_Persistent_Flag",          request.IsEventPersistentFlag },
                { "@p_Event_CONFIG_Json",                 request.EventConfigJson },
                { "@p_Status_Code",                       request.StatusCode },
                { "@p_User_ID",                           request.UserId }
            },
                    InputParameterTypes = new Dictionary<string, SqlDbType>
            {
                { "@p_Event_ID",                          SqlDbType.BigInt },
                { "@p_Parent_Event_Definition_ID",        SqlDbType.BigInt },
                { "@p_Org_ID",                            SqlDbType.Int },
                { "@p_Domain_ID",                         SqlDbType.BigInt },
                { "@p_Event_Code",                        SqlDbType.NVarChar },
                { "@p_Event_Name",                        SqlDbType.NVarChar },
                { "@p_Event_Description",                 SqlDbType.NVarChar },
                { "@p_Event_Type_Code",                   SqlDbType.NVarChar },
                { "@p_Event_Category_Code",               SqlDbType.NVarChar },
                { "@p_Event_Source_Type_Code",            SqlDbType.NVarChar },
                { "@p_Event_Source_Reference",            SqlDbType.NVarChar },
                { "@p_Event_Payload_Schema_Reference",    SqlDbType.NVarChar },
                { "@p_Event_Correlation_Key_Expression",  SqlDbType.NVarChar },
                { "@p_Event_Filter_Expression",           SqlDbType.NVarChar },
                { "@p_Rule_Set_ID",                       SqlDbType.BigInt },
                { "@p_Timeout_Duration_Seconds",          SqlDbType.Int },
                { "@p_Timeout_Action_Code",               SqlDbType.NVarChar },
                { "@p_Is_Replay_Allowed_Flag",            SqlDbType.Bit },
                { "@p_Is_Duplicate_Allowed_Flag",         SqlDbType.Bit },
                { "@p_Is_Event_Persistent_Flag",          SqlDbType.Bit },
                { "@p_Event_CONFIG_Json",                 SqlDbType.NVarChar },
                { "@p_Status_Code",                       SqlDbType.SmallInt },
                { "@p_User_ID",                           SqlDbType.Int }
            },
                    TvpParameters = new Dictionary<string, TvpParameter>
            {
                {
                    "@p_t_WF_CONFIG_Event_Action_List",
                    new TvpParameter
                    {
                        TypeName = "Workflow.t_WF_CONFIG_Event_Action",
                        Data = eventActionTable
                    }
                }
            },
                    OutputParameters = new Dictionary<string, SqlDbType>
            {
                { "@p_Output_Status_Code", SqlDbType.NVarChar },
                { "@p_Output_Status_Msg",  SqlDbType.NVarChar }
            },
                    OutputParameterSizes = new Dictionary<string, int>
            {
                { "@p_Output_Status_Code", 1000 },
                { "@p_Output_Status_Msg",  1000 }
            }
                };

                var result = await _sqlServerConnector2.ExecuteAsync(spRequest, CancellationToken.None);

                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed to execute sp_WF_CONFIG_Save_Event: {result.ErrorCode} - {result.ErrorMessage}");
                }

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters.GetValueOrDefault("@p_Output_Status_Code")?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters.GetValueOrDefault("@p_Output_Status_Msg")?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId, filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    "DB_EXCEPTION",
                    "RepositoryException",
                    ex.Message,
                    correlationId,
                    filterIds,
                    ex);
                throw;
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 3b. SaveExecutionPolicyAsync
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Execution_Policy to create or update an
        /// Execution Policy record, including its table-valued list of
        /// Execution Policy Config key/value settings.
        /// </summary>
        public async Task<CommonDBResponse> SaveExecutionPolicyAsync(SaveExecutionPolicyRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveExecutionPolicyAsync";
            var filterIds = new Dictionary<string, object> { { "ExecutionPolicyId", request.ExecutionPolicyId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId, filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId, filterIds, new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var configTable = BuildExecutionPolicyConfigDataTable(request.ExecutionPolicyConfigList);

                var spRequest = new SpExecutionRequest
                {
                    ProcedureName = "sp_WF_CONFIG_Save_Execution_Policy",
                    ServiceName = serviceName,
                    CorrelationId = correlationId,
                    InputParameters = new Dictionary<string, object?>
            {
                { "@p_Execution_Policy_ID",               request.ExecutionPolicyId },
                { "@p_Parent_Execution_Policy_ID",        request.ParentExecutionPolicyId },
                { "@p_Org_ID",                            request.OrgId },
                { "@p_Domain_ID",                         request.DomainId },
                { "@p_Execution_Policy_Code",             request.ExecutionPolicyCode },
                { "@p_Execution_Policy_Name",             request.ExecutionPolicyName },
                { "@p_Execution_Policy_Description",      request.ExecutionPolicyDescription },
                { "@p_Execution_Policy_Type_Code",        request.ExecutionPolicyTypeCode },
                { "@p_Execution_Mode_Code",               request.ExecutionModeCode },
                { "@p_Execution_Priority_Code",           request.ExecutionPriorityCode },
                { "@p_Retry_Count",                       request.RetryCount },
                { "@p_Retry_Interval_Seconds",            request.RetryIntervalSeconds },
                { "@p_Retry_Strategy_Code",               request.RetryStrategyCode },
                { "@p_Max_Retry_Interval_Seconds",        request.MaxRetryIntervalSeconds },
                { "@p_Timeout_Seconds",                   request.TimeoutSeconds },
                { "@p_Overall_Timeout_Seconds",           request.OverallTimeoutSeconds },
                { "@p_Failure_Handling_Mode_Code",        request.FailureHandlingModeCode },
                { "@p_Timeout_Handling_Mode_Code",        request.TimeoutHandlingModeCode },
                { "@p_Success_Handling_Mode_Code",        request.SuccessHandlingModeCode },
                { "@p_Queue_Reference",                   request.QueueReference },
                { "@p_Worker_Reference",                  request.WorkerReference },
                { "@p_Max_Concurrency_Count",             request.MaxConcurrencyCount },
                { "@p_Batch_Size",                        request.BatchSize },
                { "@p_Rate_Limit_Count",                  request.RateLimitCount },
                { "@p_Rate_Limit_Window_Seconds",         request.RateLimitWindowSeconds },
                { "@p_Is_IDempotency_Required_Flag",      request.IsIdempotencyRequiredFlag },
                { "@p_IDempotency_Key_Expression",        request.IdempotencyKeyExpression },
                { "@p_Is_Compensation_Required_Flag",     request.IsCompensationRequiredFlag },
                { "@p_Compensation_Action_Reference",     request.CompensationActionReference },
                { "@p_Is_Transaction_Required_Flag",      request.IsTransactionRequiredFlag },
                { "@p_Transaction_Scope_Code",            request.TransactionScopeCode },
                { "@p_Is_Reusable_Flag",                  request.IsReusableFlag },
                { "@p_Is_System_Policy_Flag",             request.IsSystemPolicyFlag },
                { "@p_Execution_CONFIG_Json",             request.ExecutionConfigJson },
                { "@p_Status_Code",                       request.StatusCode },
                { "@p_User_ID",                           request.UserId }
            },
                    InputParameterTypes = new Dictionary<string, SqlDbType>
            {
                { "@p_Execution_Policy_ID",               SqlDbType.BigInt },
                { "@p_Parent_Execution_Policy_ID",        SqlDbType.BigInt },
                { "@p_Org_ID",                            SqlDbType.Int },
                { "@p_Domain_ID",                         SqlDbType.BigInt },
                { "@p_Execution_Policy_Code",             SqlDbType.NVarChar },
                { "@p_Execution_Policy_Name",             SqlDbType.NVarChar },
                { "@p_Execution_Policy_Description",      SqlDbType.NVarChar },
                { "@p_Execution_Policy_Type_Code",        SqlDbType.NVarChar },
                { "@p_Execution_Mode_Code",               SqlDbType.NVarChar },
                { "@p_Execution_Priority_Code",           SqlDbType.NVarChar },
                { "@p_Retry_Count",                       SqlDbType.Int },
                { "@p_Retry_Interval_Seconds",            SqlDbType.Int },
                { "@p_Retry_Strategy_Code",               SqlDbType.NVarChar },
                { "@p_Max_Retry_Interval_Seconds",        SqlDbType.Int },
                { "@p_Timeout_Seconds",                   SqlDbType.Int },
                { "@p_Overall_Timeout_Seconds",           SqlDbType.Int },
                { "@p_Failure_Handling_Mode_Code",        SqlDbType.NVarChar },
                { "@p_Timeout_Handling_Mode_Code",        SqlDbType.NVarChar },
                { "@p_Success_Handling_Mode_Code",        SqlDbType.NVarChar },
                { "@p_Queue_Reference",                   SqlDbType.NVarChar },
                { "@p_Worker_Reference",                  SqlDbType.NVarChar },
                { "@p_Max_Concurrency_Count",             SqlDbType.Int },
                { "@p_Batch_Size",                        SqlDbType.Int },
                { "@p_Rate_Limit_Count",                  SqlDbType.Int },
                { "@p_Rate_Limit_Window_Seconds",         SqlDbType.Int },
                { "@p_Is_IDempotency_Required_Flag",      SqlDbType.Bit },
                { "@p_IDempotency_Key_Expression",        SqlDbType.NVarChar },
                { "@p_Is_Compensation_Required_Flag",     SqlDbType.Bit },
                { "@p_Compensation_Action_Reference",     SqlDbType.NVarChar },
                { "@p_Is_Transaction_Required_Flag",      SqlDbType.Bit },
                { "@p_Transaction_Scope_Code",            SqlDbType.NVarChar },
                { "@p_Is_Reusable_Flag",                  SqlDbType.Bit },
                { "@p_Is_System_Policy_Flag",             SqlDbType.Bit },
                { "@p_Execution_CONFIG_Json",             SqlDbType.NVarChar },
                { "@p_Status_Code",                       SqlDbType.SmallInt },
                { "@p_User_ID",                           SqlDbType.Int }
            },
                    TvpParameters = new Dictionary<string, TvpParameter>
            {
                {
                    "@p_t_WF_CONFIG_Execution_Policy_CONFIG_List",
                    new TvpParameter
                    {
                        TypeName = "Workflow.t_WF_CONFIG_Execution_Policy_CONFIG",
                        Data = configTable
                    }
                }
            },
                    OutputParameters = new Dictionary<string, SqlDbType>
            {
                { "@p_Output_Status_Code", SqlDbType.NVarChar },
                { "@p_Output_Status_Msg",  SqlDbType.NVarChar }
            },
                    OutputParameterSizes = new Dictionary<string, int>
            {
                { "@p_Output_Status_Code", 1000 },
                { "@p_Output_Status_Msg",  1000 }
            }
                };

                var result = await _sqlServerConnector2.ExecuteAsync(spRequest, CancellationToken.None);

                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed to execute sp_WF_CONFIG_Save_Execution_Policy: {result.ErrorCode} - {result.ErrorMessage}");
                }

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters.GetValueOrDefault("@p_Output_Status_Code")?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters.GetValueOrDefault("@p_Output_Status_Msg")?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId, filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    "DB_EXCEPTION",
                    "RepositoryException",
                    ex.Message,
                    correlationId,
                    filterIds,
                    ex);
                throw;
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 3c. SaveWorkflowAsync
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Workflow to create or update a Workflow definition
        /// together with its embedded Workflow Version. No TVP is used; all Version
        /// fields are passed as flat scalar parameters prefixed with @p_Version_.
        /// </summary>
        public async Task<CommonDBResponse> SaveWorkflowAsync(SaveWorkflowRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveWorkflowAsync";
            var filterIds = new Dictionary<string, object> { { "WorkflowId", request.WorkflowId ?? 0L } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId, filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId, filterIds, new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var v = request.WorkflowVersion; // convenience alias

                var spRequest = new SpExecutionRequest
                {
                    ProcedureName = "sp_WF_CONFIG_Save_Workflow",
                    ServiceName = serviceName,
                    CorrelationId = correlationId,
                    InputParameters = new Dictionary<string, object?>
            {
                // Workflow-level fields
                { "@p_Workflow_ID",                          request.WorkflowId },
                { "@p_Parent_Workflow_ID",                   request.ParentWorkflowId },
                { "@p_Workflow_Code",                        request.WorkflowCode },
                { "@p_Workflow_Name",                        request.WorkflowName },
                { "@p_Workflow_Description",                 request.WorkflowDescription },
                { "@p_Workflow_Category_Code",               request.WorkflowCategoryCode },
                { "@p_Workflow_Type_Code",                   request.WorkflowTypeCode },
                { "@p_Status_Code",                          request.StatusCode },
                // Version-level fields (flat)
                { "@p_Workflow_Version_ID",                  v?.WorkflowVersionId },
                { "@p_Version_Parent_Workflow_Version_ID",   v?.ParentWorkflowVersionId },
                { "@p_Version_Org_ID",                       v?.OrgId },
                { "@p_Version_Domain_ID",                    v?.DomainId },
                { "@p_Version_Authentication_Policy_ID",     v?.AuthenticationPolicyId },
                { "@p_Version_Authorization_Policy_ID",      v?.AuthorizationPolicyId },
                { "@p_Version_Rule_Reference_Type_Code",     v?.RuleReferenceTypeCode },
                { "@p_Version_Rule_Reference_ID",            v?.RuleReferenceId },
                { "@p_Version_Event_ID",                     v?.EventId },
                { "@p_Version_SLA_ID",                       v?.SlaId },
                { "@p_Version_Execution_Policy_ID",          v?.ExecutionPolicyId },
                { "@p_Version_Version_Number",               v?.VersionNumber },
                { "@p_Version_Version_Name",                 v?.VersionName },
                { "@p_Version_Version_Description",          v?.VersionDescription },
                { "@p_Version_Effective_From_Date",          v?.EffectiveFromDate },
                { "@p_Version_Effective_To_Date",            v?.EffectiveToDate },
                { "@p_Version_Is_Published_Flag",            v?.IsPublishedFlag },
                { "@p_Version_Is_Current_Flag",              v?.IsCurrentFlag },
                { "@p_Version_Validated_Date_Time",          v?.ValidatedDateTime },
                { "@p_Version_Published_Date_Time",          v?.PublishedDateTime },
                { "@p_Version_Status_Code",                  v?.VersionStatusCode },
                { "@p_User_ID",                              request.UserId }
            },
                    InputParameterTypes = new Dictionary<string, SqlDbType>
            {
                { "@p_Workflow_ID",                          SqlDbType.BigInt },
                { "@p_Parent_Workflow_ID",                   SqlDbType.BigInt },
                { "@p_Workflow_Code",                        SqlDbType.NVarChar },
                { "@p_Workflow_Name",                        SqlDbType.NVarChar },
                { "@p_Workflow_Description",                 SqlDbType.NVarChar },
                { "@p_Workflow_Category_Code",               SqlDbType.NVarChar },
                { "@p_Workflow_Type_Code",                   SqlDbType.NVarChar },
                { "@p_Status_Code",                          SqlDbType.SmallInt },
                { "@p_Workflow_Version_ID",                  SqlDbType.BigInt },
                { "@p_Version_Parent_Workflow_Version_ID",   SqlDbType.BigInt },
                { "@p_Version_Org_ID",                       SqlDbType.Int },
                { "@p_Version_Domain_ID",                    SqlDbType.BigInt },
                { "@p_Version_Authentication_Policy_ID",     SqlDbType.BigInt },
                { "@p_Version_Authorization_Policy_ID",      SqlDbType.BigInt },
                { "@p_Version_Rule_Reference_Type_Code",     SqlDbType.NVarChar },
                { "@p_Version_Rule_Reference_ID",            SqlDbType.BigInt },
                { "@p_Version_Event_ID",                     SqlDbType.BigInt },
                { "@p_Version_SLA_ID",                       SqlDbType.BigInt },
                { "@p_Version_Execution_Policy_ID",          SqlDbType.BigInt },
                { "@p_Version_Version_Number",               SqlDbType.NVarChar },
                { "@p_Version_Version_Name",                 SqlDbType.NVarChar },
                { "@p_Version_Version_Description",          SqlDbType.NVarChar },
                { "@p_Version_Effective_From_Date",          SqlDbType.DateTime },
                { "@p_Version_Effective_To_Date",            SqlDbType.DateTime },
                { "@p_Version_Is_Published_Flag",            SqlDbType.Bit },
                { "@p_Version_Is_Current_Flag",              SqlDbType.Bit },
                { "@p_Version_Validated_Date_Time",          SqlDbType.DateTime },
                { "@p_Version_Published_Date_Time",          SqlDbType.DateTime },
                { "@p_Version_Status_Code",                  SqlDbType.NVarChar },
                { "@p_User_ID",                              SqlDbType.Int }
            },
                    OutputParameters = new Dictionary<string, SqlDbType>
            {
                { "@p_Output_Status_Code", SqlDbType.NVarChar },
                { "@p_Output_Status_Msg",  SqlDbType.NVarChar }
            },
                    OutputParameterSizes = new Dictionary<string, int>
            {
                { "@p_Output_Status_Code", 1000 },
                { "@p_Output_Status_Msg",  1000 }
            }
                };

                var result = await _sqlServerConnector2.ExecuteAsync(spRequest, CancellationToken.None);

                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed to execute sp_WF_CONFIG_Save_Workflow: {result.ErrorCode} - {result.ErrorMessage}");
                }

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters.GetValueOrDefault("@p_Output_Status_Code")?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters.GetValueOrDefault("@p_Output_Status_Msg")?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId, filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    "DB_EXCEPTION",
                    "RepositoryException",
                    ex.Message,
                    correlationId,
                    filterIds,
                    ex);
                throw;
            }
        }

        // ──────────────────────────────────────────────────────────────
        // 3d. SaveWorkflowTransitionsAsync
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Calls sp_WF_CONFIG_Save_Workflow_Transitions to replace the full set of
        /// Workflow Transitions for a given Workflow Version. The SP is entirely
        /// table-valued: a Workflow_Version_ID scalar plus a transition TVP.
        /// </summary>
        public async Task<CommonDBResponse> SaveWorkflowTransitionsAsync(SaveWorkflowTransitionsRequest request, string correlationId)
        {
            string serviceName = "WorkflowConfigRepository.SaveWorkflowTransitionsAsync";
            var filterIds = new Dictionary<string, object> { { "WorkflowVersionId", request.WorkflowVersionId } };

            _logger.LogInformation(serviceName, "Repository Started", correlationId, filterIds);
            _logger.LogDebug(serviceName, "Input Data", correlationId, filterIds, new Dictionary<string, object> { { "Request", request! } });

            try
            {
                var transitionTable = BuildWorkflowTransitionDataTable(request.TransitionList);

                var spRequest = new SpExecutionRequest
                {
                    ProcedureName = "sp_WF_CONFIG_Save_Workflow_Transitions",
                    ServiceName = serviceName,
                    CorrelationId = correlationId,
                    InputParameters = new Dictionary<string, object?>
            {
                { "@p_Workflow_Version_ID", request.WorkflowVersionId },
                { "@p_User_ID",             request.UserId }
            },
                    InputParameterTypes = new Dictionary<string, SqlDbType>
            {
                { "@p_Workflow_Version_ID", SqlDbType.BigInt },
                { "@p_User_ID",             SqlDbType.Int }
            },
                    TvpParameters = new Dictionary<string, TvpParameter>
            {
                {
                    "@p_Transition_List",
                    new TvpParameter
                    {
                        TypeName = "Workflow.t_WF_CONFIG_Workflow_Transition",
                        Data = transitionTable
                    }
                }
            },
                    OutputParameters = new Dictionary<string, SqlDbType>
            {
                { "@p_Output_Status_Code", SqlDbType.NVarChar },
                { "@p_Output_Status_Msg",  SqlDbType.NVarChar }
            },
                    OutputParameterSizes = new Dictionary<string, int>
            {
                { "@p_Output_Status_Code", 1000 },
                { "@p_Output_Status_Msg",  1000 }
            }
                };

                var result = await _sqlServerConnector2.ExecuteAsync(spRequest, CancellationToken.None);

                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed to execute sp_WF_CONFIG_Save_Workflow_Transitions: {result.ErrorCode} - {result.ErrorMessage}");
                }

                var response = new CommonDBResponse
                {
                    OutputStatusCode = int.TryParse(result.OutputParameters.GetValueOrDefault("@p_Output_Status_Code")?.ToString(), out var code) ? code : 0,
                    OutputStatusMsg = result.OutputParameters.GetValueOrDefault("@p_Output_Status_Msg")?.ToString(),
                    Id = result?.ResultSets?.FirstOrDefault()
                };

                _logger.LogInformation(serviceName, "Repository Ended", correlationId, filterIds);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    serviceName,
                    "Repository Failed",
                    "DB_EXCEPTION",
                    "RepositoryException",
                    ex.Message,
                    correlationId,
                    filterIds,
                    ex);
                throw;
            }
        }


        // ============================================================
        // SECTION 4 — DataTable BUILDER HELPERS (WorkflowConfigRepository.cs)
        // Add these private helpers alongside the existing
        // BuildAuthorizationPolicyRuleDataTable helper.
        // ============================================================

        /// <summary>
        /// Builds the DataTable for the [Workflow].[t_WF_CONFIG_Event_Action] TVP.
        /// </summary>
        private static DataTable BuildEventActionDataTable(List<EventActionItem> items)
        {
            var table = new DataTable();
            table.Columns.Add("Event_Action_ID", typeof(long));
            table.Columns.Add("Org_ID", typeof(int));
            table.Columns.Add("Domain_ID", typeof(long));
            table.Columns.Add("Event_Action_Code", typeof(string));
            table.Columns.Add("Event_Action_Name", typeof(string));
            table.Columns.Add("Event_Action_Description", typeof(string));
            table.Columns.Add("Event_Action_Type_Code", typeof(string));
            table.Columns.Add("Event_Action_Category_Code", typeof(string));
            table.Columns.Add("Event_Action_Reference_Type_Code", typeof(string));
            table.Columns.Add("Event_Action_Reference_ID", typeof(long));
            table.Columns.Add("Event_Action_Reference_Value", typeof(string));
            table.Columns.Add("Outcome_Code", typeof(string));
            table.Columns.Add("Condition_Expression", typeof(string));
            table.Columns.Add("Rule_Set_ID", typeof(long));
            table.Columns.Add("Execution_Policy_ID", typeof(long));
            table.Columns.Add("Sequence_Number", typeof(int));
            table.Columns.Add("Is_Mandatory_Flag", typeof(bool));
            table.Columns.Add("Continue_On_Failure_Flag", typeof(bool));
            table.Columns.Add("Is_Idempotent_Flag", typeof(bool));
            table.Columns.Add("Event_Action_CONFIG_Json", typeof(string));
            table.Columns.Add("Status_Code", typeof(short));

            foreach (var item in items ?? [])
            {
                table.Rows.Add(
                    item.EventActionId as object ?? DBNull.Value,
                    item.OrgId,
                    item.DomainId as object ?? DBNull.Value,
                    item.EventActionCode as object ?? DBNull.Value,
                    item.EventActionName as object ?? DBNull.Value,
                    item.EventActionDescription as object ?? DBNull.Value,
                    item.EventActionTypeCode as object ?? DBNull.Value,
                    item.EventActionCategoryCode as object ?? DBNull.Value,
                    item.EventActionReferenceTypeCode as object ?? DBNull.Value,
                    item.EventActionReferenceId as object ?? DBNull.Value,
                    item.EventActionReferenceValue as object ?? DBNull.Value,
                    item.OutcomeCode as object ?? DBNull.Value,
                    item.ConditionExpression as object ?? DBNull.Value,
                    item.RuleSetId as object ?? DBNull.Value,
                    item.ExecutionPolicyId as object ?? DBNull.Value,
                    item.SequenceNumber as object ?? DBNull.Value,
                    item.IsMandatoryFlag as object ?? DBNull.Value,
                    item.ContinueOnFailureFlag as object ?? DBNull.Value,
                    item.IsIdempotentFlag as object ?? DBNull.Value,
                    item.EventActionConfigJson as object ?? DBNull.Value,
                    item.StatusCode as object ?? DBNull.Value
                );
            }

            return table;
        }

        /// <summary>
        /// Builds the DataTable for the [Workflow].[t_WF_CONFIG_Execution_Policy_CONFIG] TVP.
        /// </summary>
        private static DataTable BuildExecutionPolicyConfigDataTable(List<ExecutionPolicyConfigItem> items)
        {
            var table = new DataTable();
            table.Columns.Add("Execution_Config_ID", typeof(long));
            table.Columns.Add("Config_Key", typeof(string));
            table.Columns.Add("Config_Value", typeof(string));
            table.Columns.Add("Config_Data_Type_Code", typeof(string));
            table.Columns.Add("Config_Value_Unit_Code", typeof(string));
            table.Columns.Add("Config_Category_Code", typeof(string));
            table.Columns.Add("Config_Description", typeof(string));
            table.Columns.Add("Is_Required_Flag", typeof(bool));
            table.Columns.Add("Is_Sensitive_Flag", typeof(bool));
            table.Columns.Add("Display_Order", typeof(int));
            table.Columns.Add("Validation_Expression", typeof(string));
            table.Columns.Add("Default_Value", typeof(string));
            table.Columns.Add("Allowed_Values", typeof(string));
            table.Columns.Add("Environment_Scope_Code", typeof(string));
            table.Columns.Add("Status_Code", typeof(short));

            foreach (var item in items ?? [])
            {
                table.Rows.Add(
                    item.ExecutionConfigId as object ?? DBNull.Value,
                    item.ConfigKey as object ?? DBNull.Value,
                    item.ConfigValue as object ?? DBNull.Value,
                    item.ConfigDataTypeCode as object ?? DBNull.Value,
                    item.ConfigValueUnitCode as object ?? DBNull.Value,
                    item.ConfigCategoryCode as object ?? DBNull.Value,
                    item.ConfigDescription as object ?? DBNull.Value,
                    item.IsRequiredFlag as object ?? DBNull.Value,
                    item.IsSensitiveFlag as object ?? DBNull.Value,
                    item.DisplayOrder as object ?? DBNull.Value,
                    item.ValidationExpression as object ?? DBNull.Value,
                    item.DefaultValue as object ?? DBNull.Value,
                    item.AllowedValues as object ?? DBNull.Value,
                    item.EnvironmentScopeCode as object ?? DBNull.Value,
                    item.StatusCode as object ?? DBNull.Value
                );
            }

            return table;
        }

        /// <summary>
        /// Builds the DataTable for the [Workflow].[t_WF_CONFIG_Workflow_Transition] TVP.
        /// </summary>
        private static DataTable BuildWorkflowTransitionDataTable(List<WorkflowTransitionItem> items)
        {
            var table = new DataTable();
            table.Columns.Add("Workflow_Transition_ID", typeof(long));
            table.Columns.Add("From_Node_Type_Code", typeof(string));
            table.Columns.Add("From_Node_ID", typeof(long));
            table.Columns.Add("To_Node_Type_Code", typeof(string));
            table.Columns.Add("To_Node_ID", typeof(long));
            table.Columns.Add("Transition_Code", typeof(string));
            table.Columns.Add("Transition_Name", typeof(string));
            table.Columns.Add("Transition_Description", typeof(string));
            table.Columns.Add("Transition_Mode_Code", typeof(string));
            table.Columns.Add("Transition_Path_Type_Code", typeof(string));
            table.Columns.Add("Sequence_Number", typeof(int));
            table.Columns.Add("Rule_Reference_Type_Code", typeof(string));
            table.Columns.Add("Rule_Reference_ID", typeof(long));
            table.Columns.Add("Branch_Group_Code", typeof(string));
            table.Columns.Add("Branch_Completion_Policy_Code", typeof(string));
            table.Columns.Add("Required_Completion_Count", typeof(int));
            table.Columns.Add("Event_ID", typeof(long));
            table.Columns.Add("Delay_Duration_Seconds", typeof(int));
            table.Columns.Add("Is_Default_Path_Flag", typeof(bool));
            table.Columns.Add("Is_Start_Transition_Flag", typeof(bool));
            table.Columns.Add("Is_Terminal_Transition_Flag", typeof(bool));
            table.Columns.Add("Status_Code", typeof(short));

            foreach (var item in items ?? [])
            {
                table.Rows.Add(
                    item.WorkflowTransitionId as object ?? DBNull.Value,
                    item.FromNodeTypeCode as object ?? DBNull.Value,
                    item.FromNodeId as object ?? DBNull.Value,
                    item.ToNodeTypeCode as object ?? DBNull.Value,
                    item.ToNodeId as object ?? DBNull.Value,
                    item.TransitionCode as object ?? DBNull.Value,
                    item.TransitionName as object ?? DBNull.Value,
                    item.TransitionDescription as object ?? DBNull.Value,
                    item.TransitionModeCode as object ?? DBNull.Value,
                    item.TransitionPathTypeCode as object ?? DBNull.Value,
                    item.SequenceNumber as object ?? DBNull.Value,
                    item.RuleReferenceTypeCode as object ?? DBNull.Value,
                    item.RuleReferenceId as object ?? DBNull.Value,
                    item.BranchGroupCode as object ?? DBNull.Value,
                    item.BranchCompletionPolicyCode as object ?? DBNull.Value,
                    item.RequiredCompletionCount as object ?? DBNull.Value,
                    item.EventId as object ?? DBNull.Value,
                    item.DelayDurationSeconds as object ?? DBNull.Value,
                    item.IsDefaultPathFlag as object ?? DBNull.Value,
                    item.IsStartTransitionFlag as object ?? DBNull.Value,
                    item.IsTerminalTransitionFlag as object ?? DBNull.Value,
                    item.StatusCode as object ?? DBNull.Value
                );
            }

            return table;
        }

    }
}
