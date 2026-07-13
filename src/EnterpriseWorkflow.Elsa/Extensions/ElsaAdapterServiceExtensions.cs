using Acornima.Ast;
using Elsa.Alterations.Extensions;
using Elsa.EntityFrameworkCore.Extensions;
using Elsa.EntityFrameworkCore.Modules.Management;
using Elsa.EntityFrameworkCore.Modules.Runtime;
using Elsa.Extensions;
using Elsa.Mediator.Contracts;
using Elsa.Workflows.Notifications;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Application.Services;
using EnterpriseWorkflow.Elsa.Adapters;
using EnterpriseWorkflow.Elsa.Mapping;
using EnterpriseWorkflow.Elsa.Notifications;
using EnterpriseWorkflow.Elsa.RuleEngineAdapter;
using EnterpriseWorkflow.Storage.Contracts;
using EnterpriseWorkflow.Storage.DbContext;
using EnterpriseWorkflow.Storage.Implementations.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NdRulesEngine;
using Quartz;

namespace EnterpriseWorkflow.Elsa.Extensions;

public static class ElsaAdapterServiceExtensions
{
    public static IServiceCollection AddElsaWorkflowAdapter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── 1. Validate config keys at startup — fail fast with a clear message ──
        var elsaDbConnectionString = configuration.GetConnectionString("ElsaDb")
            ?? throw new InvalidOperationException(
                "Missing required config: ConnectionStrings:ElsaDb (SQL Server connection string)");

        var enterpriseConnectionString = configuration.GetConnectionString("EnterpriseDb")
            ?? throw new InvalidOperationException(
                "Missing required config: ConnectionStrings:EnterpriseDb (SQL Server connection string)");

        // This is the ONLY place that needs an HTTP URL — Elsa REST API for publish/import
        var elsaServerUrl = configuration["Elsa:ServerUrl"]
            ?? throw new InvalidOperationException(
                "Missing required config: Elsa:ServerUrl  " +
                "Expected an HTTP URL e.g. 'http://localhost:5000'  " +
                "This is NOT the same as ConnectionStrings:ElsaDb");

        if (!Uri.TryCreate(elsaServerUrl, UriKind.Absolute, out var elsaUri)
            || (elsaUri.Scheme != Uri.UriSchemeHttp
                && elsaUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException(
                $"Elsa:ServerUrl '{elsaServerUrl}' is not a valid HTTP/HTTPS URL. " +
                $"Check appsettings.json — ConnectionStrings:ElsaDb is the SQL string " +
                $"and Elsa:ServerUrl is the REST API base address.");
        }

        // ── 2. Quartz — SQL-backed scheduler (unchanged from your original) ──────
        services.AddQuartz(q =>
        {
            q.SchedulerId = "AUTO";
            q.Properties["quartz.jobStore.performSchemaValidation"] = "false";
            q.UsePersistentStore(s =>
            {
                s.UseProperties = true;
                s.RetryInterval = TimeSpan.FromSeconds(15);
                s.UseSqlServer(elsaDbConnectionString);   // SQL string — correct
                s.UseSystemTextJsonSerializer();
                s.UseClustering();
            });
        });
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = false);

        // ── 3. HttpClient for Elsa REST API (publish/import only) ───────────────
        //    BaseAddress = HTTP URL from Elsa:ServerUrl — NOT the SQL string
        services.AddHttpClient("ElsaServer", c =>
        {
            c.BaseAddress = elsaUri;                      // HTTP URL — correct
            c.DefaultRequestHeaders.Add("Accept", "application/json");
            // Uncomment if your Elsa server requires an API key:
            // var apiKey = configuration["Elsa:ApiKey"];
            // if (!string.IsNullOrEmpty(apiKey))
            //     c.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
        });

        // ── 4. Elsa engine (unchanged from your original) ────────────────────────
        services
            .AddElsa(elsa => elsa
                .UseIdentity(identity =>
                {
                    identity.TokenOptions = options =>
                        options.SigningKey = configuration["Elsa:SigningKey"]!;
                    identity.UseAdminUserProvider();
                })
                .UseDefaultAuthentication()
                .UseWorkflowManagement(m => m.UseEntityFrameworkCore(ef =>
                    ef.UseSqlServer(elsaDbConnectionString)))
                .UseWorkflowRuntime(r => r.UseEntityFrameworkCore(ef =>
                    ef.UseSqlServer(elsaDbConnectionString)))
                .UseScheduling(scheduling => scheduling.UseQuartzScheduler())
                .UseJavaScript()
                .UseCSharp()
                .UseWorkflowsApi()
                .UseHttp(http => http.ConfigureHttpOptions = options =>
                    configuration.GetSection("Http").Bind(options))
                .AddActivitiesFrom<EvaluateRuleActivity>()
                .AddActivitiesFrom<Program>()
                .UseQuartz()
            );

        // ── 5. Enterprise ADO.NET repositories ───────────────────────────────────
        services.AddScoped<IConfigNodeLookupRepository, ConfigNodeLookupRepository>();

        services.AddScoped<IWorkflowInstanceRepository, WorkflowInstanceRepository>();

        services.AddScoped<IWorkflowExecutionRepository, WorkflowExecutionRepository>();

        services.AddScoped<IStatusHistoryRepository, StatusHistoryRepository>();

        services.AddScoped<INodeInstanceRepository, NodeInstanceRepository>();
            
        services.AddScoped<INodeExecutionRepository, NodeExecutionRepository>();

        services.AddScoped<IWaitStateRepository, WaitStateRepository>();


        // ── 6. Application services ───────────────────────────────────────────────
        services.AddScoped<EnterpriseToElsaMapper>();
        services.AddScoped<WorkflowActivityLogRepository>();
        services.AddScoped<IWorkflowPublishService, WorkflowPublishService>();

        // ── 7. Elsa notification handlers (unchanged from your original) ──────────
        services.Scan(scan => scan
            .FromAssemblies(typeof(ElsaNotificationHandler).Assembly)
            .AddClasses(classes =>
                classes.AssignableTo(typeof(INotificationHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

 

        return services;
    }

    public static IServiceCollection AddRuleEngine(
        this IServiceCollection services)
    {


        services.AddNdRulesEngine();

        var provider = services.BuildServiceProvider();

        var registry = provider.GetRequiredService<INdPluginRegistry>();
        registry.ScanAssembly(typeof(Program).Assembly);

        //var engine = provider.GetRequiredService<INdRuleEngine>();
        //engine.LoadWorkflowFromJson(File.ReadAllText("rules.json"));

        services.TryAddSingleton<IRuleInputBuilder, RuleInputBuilder>();

        services.TryAddSingleton<IRuleEngineExecutionService, NdBackedRuleEngineExecutionService>();

        //const string ruleDefinitionsPath = "Stubs/RuleDefinitions.json";

        //services.Configure<JsonRuleRepositoryOptions>(
        //    o => o.FilePath = ruleDefinitionsPath);

        services.TryAddSingleton<IRuleRepository, JsonRuleRepository>();
        services.TryAddSingleton<IRuleInputBuilder, RuleInputBuilder>();
        //services.TryAddSingleton<IRuleEngineExecutionService,
        //    DbBackedRuleEngineExecutionService>();


        return services;
    }
}