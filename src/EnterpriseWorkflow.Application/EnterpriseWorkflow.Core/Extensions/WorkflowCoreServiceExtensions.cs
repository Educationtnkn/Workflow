using Acornima.Ast;
using Elsa.Workflows.LogPersistence.Strategies;
using Elsa.Workflows.Management;
using EnterpriseWorkflow.Application.Ports.Inbound.Interface;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Application.Services;
using EnterpriseWorkflow.Application.Stubs;
//using EnterpriseWorkflow.Application.Stubs;
using EnterpriseWorkflow.Elsa.Adapters;
using EnterpriseWorkflow.Elsa.Implementations;
using EnterpriseWorkflow.Storage.Contracts;
using EnterpriseWorkflow.Storage.DbContext;
using EnterpriseWorkflow.Storage.Implementations.Repositories;
using EnterpriseWorkflow.Storage.Implementations.Repositories.SqlServer;
using EnterpriseWorkflow.Storage.Implementations.Services;
using EnterpriseWorkflow.Storage.Interfaces.Repositories;
using EnterpriseWorkflow.Storage.Interfaces.Services;
using EnterpriseWorkflow.Utilities.CommonUtility;
using EnterpriseWorkflow.Utilities.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NdRulesEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Core.Extensions
{
    public static class WorkflowCoreServiceExtensions
    {
                public static IServiceCollection AddWorkflowCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register core services

            services.AddScoped<IWorkflowEngineAdapter, ElsaWorkflowAdapter>();
           // services.AddScoped<IWorkflowOrchestrationService, WorkflowExecutionEngine>();
            services.AddScoped<IAuthorizationHandler, DummyAuthorizationHandler>();

            // Register repositories
            services.AddScoped<IWorkflowDefinitionRepository, WorkflowDefinitionRepository>();
            //services.AddScoped<IWorkflowExecutionRepository, WorkflowExecutionRepository>();
            //services.AddScoped<IWaitStateRepository, WaitStateRepository>();
            services.AddScoped<ISecurityPolicyStore, DummySecurityPolicyStore>();

            // also, if needed:
            services.AddScoped<IAuthorizationHandler, DummyAuthorizationHandler>();
            //builder.Services.AddScoped<IWorkflowDefinitionRepository, DummyWorkflowDefinitionRepository>();
            services.AddScoped<ISecurityPolicyStore, DummySecurityPolicyStore>();
            //builder.Services.AddScoped<IAuthorizationHandler, DummyAuthorizationHandler>();  // <-- ADD THIS
            services.AddScoped<IWorkflowDefinition, WorkflowDefinitionService>();
            services.AddScoped<IWorkflowExecution, WorkflowExecutionService>();
            services.AddScoped<IWorkflowExecutionRepository, WorkflowExecutionRepository>();
            services.AddScoped<IGetWorkflow, GetWorkflowService>();
            services.AddScoped<IGetReportSummary, GetReportSummaryService>();
            services.AddScoped<IActivityStatusMappingService, ActivityStatusMappingService>();
            // Register execution context factory
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<CorrelationIdHelper>();
            services.AddScoped<IWorkflowCreationService, WorkflowCreationService>();
            services.AddScoped<IWorkflowCreationRepository, WorkflowCreationRepository>();
            // services.AddScoped<ExecutionContextBuilder>();


            services.AddScoped<IWorkflowExecution, WorkflowExecutionService>();
            services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();
            services.AddScoped<IStatusHistoryRepository, StatusHistoryRepository>();
            services.AddScoped<IElsaWorkflowStarter, ElsaWorkflowStarter>();
            services.AddScoped<INodeInstanceRepository, NodeInstanceRepository>();
            services.AddScoped<INodeExecutionRepository, NodeExecutionRepository>();

            services.AddScoped<IWaitStateRepository, WaitStateRepository>();

            services.AddScoped<IWorkflowPublishService, WorkflowPublishService>();

            services.AddScoped<IBuildPublishElsaDefinition, BuildPublishElsaDefinition>();

            services.AddScoped<IConfigNodeLookupRepository, ConfigNodeLookupRepository>();

            // WorkflowCoreServiceExtensions.AddWorkflowCore — register
            services.AddScoped<IWorkflowEventTriggerService, WorkflowEventTriggerService>();


            //services.AddHttpClient("ElsaServer", c =>
            //{
            //    c.BaseAddress = new Uri(configuration["ConnectionStrings:ElsaDb"]!);
            //    // add auth header if your Elsa instance requires it
            //});

            return services;
        }
    }
}
