using Elsa.Workflows;
using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Management.Models;
using Elsa.Workflows.Management.Services;
using EnterpriseWorkflow.Application.Model;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Elsa.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static EnterpriseWorkflow.Application.Ports.Outbound.Implemenation.WorkFlowEngine;

namespace EnterpriseWorkflow.Elsa.Adapters;

    public sealed class BuildPublishElsaDefinition : IBuildPublishElsaDefinition
    {
    private readonly HttpClient _elsaClient;
    private readonly string _apiPrefix;
    private readonly ILogger<BuildPublishElsaDefinition> _logger;
    private readonly IConfiguration _configuration;

    private readonly IWorkflowDefinitionImporter _importer;
    private readonly IApiSerializer _apiSerializer;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public BuildPublishElsaDefinition(IHttpClientFactory httpFactory,
        IConfiguration configuration,
        ILogger<BuildPublishElsaDefinition> logger, IWorkflowDefinitionImporter workflowDefinitionImporter, IApiSerializer apiSerializer)
        {
        _elsaClient = httpFactory.CreateClient("ElsaServer");
        _apiPrefix = (configuration["Elsa:ServerUrl"] ?? "elsa/api").TrimEnd('/');
        _logger = logger;
        _configuration = configuration;
        _importer = workflowDefinitionImporter;
        _apiSerializer = apiSerializer;
    }

       public async Task<WorkflowPublishResult> BuildElsaDefinition(
        WorkflowPublishRequest request, CancellationToken ct)
    {
        try
        {
            var token = await LoginAsync(ct);
            if (token is null)
                return new WorkflowPublishResult(false, "Elsa login failed");

            _elsaClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            // Build Elsa-specific JSON — all Elsa knowledge is here
            var definition = BuildElsaDefinition(request);


            var json = JsonSerializer.Serialize(new { model = definition }, JsonOpts); // wrap in "model" like ConvertToElsaJson does

            var saveRequest = _apiSerializer.Deserialize<SaveWorkflowDefinitionRequest>(json);
            var importResult = await _importer.ImportAsync(saveRequest, ct);

            if (!importResult.Succeeded)
            {
                var errors = string.Join("; ", importResult.ValidationErrors.Select(e => e.Message));
                _logger.LogError("[ElsaPublishAdapter] Import failed: {Errors}", errors);
                return new WorkflowPublishResult(false, errors);
            }

            //var importUrl = $"{_apiPrefix}/workflow-definitions/{definition.DefinitionId}/import";
            //var publishUrl = $"{_apiPrefix}/workflow-definitions/{definition.DefinitionId}/publish";

            //_logger.LogInformation(
            //    "[ElsaPublishAdapter] Import  PUT  {Base}{Url}",
            //    _elsaClient.BaseAddress, importUrl);

            //var importResponse = await _elsaClient.PutAsJsonAsync(
            //    importUrl, definition, JsonOpts, ct);

        

            //if (!importResponse.IsSuccessStatusCode)
            //{
            //    var body = await importResponse.Content.ReadAsStringAsync(ct);
            //    _logger.LogError(
            //        "[ElsaPublishAdapter] Import failed {Status} {Url} {Body}",
            //        (int)importResponse.StatusCode, importUrl, body);

            //    return new WorkflowPublishResult(false,
            //        $"Elsa import {(int)importResponse.StatusCode}: {body}");
            //}

            //_logger.LogInformation(
            //    "[ElsaPublishAdapter] Publish POST {Base}{Url}",
            //    _elsaClient.BaseAddress, publishUrl);

            //var publishResponse = await _elsaClient.PostAsync(publishUrl, null, ct);

            //if (!publishResponse.IsSuccessStatusCode)
            //{
            //    var body = await publishResponse.Content.ReadAsStringAsync(ct);
            //    _logger.LogError(
            //        "[ElsaPublishAdapter] Publish failed {Status} {Url} {Body}",
            //        (int)publishResponse.StatusCode, publishUrl, body);

            //    return new WorkflowPublishResult(false,
            //        $"Elsa publish {(int)publishResponse.StatusCode}: {body}");
            //}

            return new WorkflowPublishResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ElsaPublishAdapter] Unexpected error");
            return new WorkflowPublishResult(false, ex.Message);
        }
    }

    // ── All Elsa JSON building is private to this adapter ────────────────────

    private ElsaWorkflowDefinition BuildElsaDefinition(WorkflowPublishRequest req)
    {
        var activities = req.Nodes
            .OrderBy(n => n.SequenceNumber)
            .Select(n => BuildActivity(n, req.WorkflowCode))
            .ToList();

        var connections = req.Transitions
            .OrderBy(t => t.SequenceNumber)
            .Select(t => BuildConnection(t, req.Nodes))
            .Where(c => c is not null)
            .Cast<ElsaConnection>()
            .ToList();

        return new ElsaWorkflowDefinition
        {
            DefinitionId = req.WorkflowCode,
            Name = req.WorkflowCode,
            Description = req.WorkflowDescription,
            Variables = new List<ElsaVariable>
            {
                new() { Id = "var-tenantId",     Name = "tenantId",
                        Value = req.OrgId.ToString() },
                new() { Id = "var-domainId",     Name = "domainId",
                        Value = req.DomainId.ToString() },
                new() { Id = "var-FwaDecisions", Name = "FwaDecisions",
                        Value = null }
            },
            Root = new ElsaFlowchartRoot
            {
                Id = $"{req.WorkflowCode}_Flowchart",
                NodeId = $"Workflow1:{req.WorkflowCode}_Flowchart",
                Activities = activities,
                Connections = connections
            }
        };
    }

    private static Dictionary<string, object> BuildActivity(
        WorkflowNodeDto node, string workflowCode)
    {
        var config = JsonSerializer.Deserialize<Dictionary<string, object>>(
            node.ConfigJson ?? "{}",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? new();

      var engineType = config.TryGetValue("engineType", out var et)
    ? et?.ToString() ?? "Unknown" : "Unknown";

        var activity = new Dictionary<string, object>
        {
            ["id"] = node.EngineActivityReference,
            ["nodeId"] = $"Workflow1:{workflowCode}_Flowchart:{node.EngineActivityReference}",
            ["type"] = engineType,
            ["version"] = 1,
            ["customProperties"] = new Dictionary<string, object>
            {
                ["canStartWorkflow"] = false,
                ["runAsynchronously"] = false
            },
            ["metadata"] = config.TryGetValue("designer", out var d)
                ? new Dictionary<string, object> { ["designer"] = d }
                : new Dictionary<string, object>()
        };

        ApplyTypeSpecificProperties(engineType, config, activity);
        return activity;
    }

    private static void ApplyTypeSpecificProperties(
        string engineType,
        Dictionary<string, object> config,
        Dictionary<string, object> activity)
    {
        string? S(string key) =>
            config.TryGetValue(key, out var v) ? v?.ToString() : null;

        bool B(string key, bool def = true)
        {
            if (!config.TryGetValue(key, out var v)) return def;
            if (v is JsonElement je) return je.GetBoolean();
            return bool.TryParse(v?.ToString(), out var r) ? r : def;
        }

        switch (engineType)
        {
            case "Elsa.RunJavaScript":
                activity["script"] = Expr("String", "JavaScript", S("script") ?? "");
                activity["result"] = (object?)null!;
                break;

            case "Elsa.ExecuteWorkflow":
                activity["workflowDefinitionId"] =
                    Expr("String", "Literal", S("workflowDefinitionId") ?? "");
                activity["input"] =
                    Expr("ObjectDictionary", S("inputExpressionType") ?? "JavaScript",
                         S("inputExpression") ?? "");
                activity["waitForCompletion"] =
                    Expr("Boolean", "Literal", B("waitForCompletion"));
                activity["correlationId"] = (object?)null!;
                activity["result"] = (object?)null!;
                break;

            case "Elsa.FlowDecision":
                activity["condition"] =
                    Expr("Boolean", "JavaScript", S("conditionExpression") ?? "false");
                break;

            case "Elsa.FlowFork":
                activity["branches"] = (object?)null!;
                break;

            case "Elsa.FlowJoin":
                activity["mode"] = new Dictionary<string, object>
                {
                    ["typeName"] = "Elsa.Workflows.Activities.Flowchart.Models.FlowJoinMode, Elsa.Workflows.Core",
                    ["expression"] = new Dictionary<string, object>
                    { ["type"] = "Literal", ["value"] = S("mode") ?? "WaitAll" }
                };
                if (config.TryGetValue("name", out var nm) && nm is not null)
                    activity["name"] = nm;
                break;

            case "Elsa.WriteLine":
                var txt = S("textExpression") ?? "";
                activity["text"] = Expr("String",
                    txt.StartsWith('`') || txt.Contains("getVariable")
                        ? "JavaScript" : "Literal", txt);
                break;

            case "Elsa.Event":
                activity["eventName"] = Expr("String", "Literal", S("eventName") ?? "");
                activity["result"] = (object?)null!;
                break;

            case "MyApp.EvaluateRuleActivity":
                activity["ruleWorkflow"] = Expr("String", "Literal", S("ruleWorkflow") ?? "");
                activity["outputVariable"] = Expr("String", "Literal", S("outputVariable") ?? "");
                activity["ruleNames"] = (object?)null!;
                break;
        }
    }

    private static Dictionary<string, object> Expr(string typeName, string exprType, object value)
        => new()
        {
            ["typeName"] = typeName,
            ["expression"] = new Dictionary<string, object>
            { ["type"] = exprType, ["value"] = value }
        };

    private static ElsaConnection? BuildConnection(
        WorkflowTransitionDto t, List<WorkflowNodeDto> nodes)
    {
        var from = nodes.FirstOrDefault(n =>
            n.NodeId == t.FromNodeId && n.NodeTableType == t.FromNodeType);
        var to = nodes.FirstOrDefault(n =>
            n.NodeId == t.ToNodeId && n.NodeTableType == t.ToNodeType);

        if (from is null || to is null) return null;

        var port = t.TransitionPathType switch
        {
            "TRUE" => "True",
            "FALSE" => "False",
            _ => "Done"
        };

        return new ElsaConnection
        {
            Source = new ElsaConnectionEndpoint
            { Activity = from.EngineActivityReference, Port = port },
            Target = new ElsaConnectionEndpoint
            { Activity = to.EngineActivityReference, Port = "In" }
        };
    }

    private sealed class ElsaLoginResponse
    {
        public string AccessToken { get; set; } = default!;
    }

    private async Task<string?> LoginAsync(CancellationToken ct)
    {
        var loginPayload = new
        {
            Username = _configuration["Elsa:Identity:Admin:Username"],
            Password = _configuration["Elsa:Identity:Admin:Password"]
        };

        var response = await _elsaClient.PostAsJsonAsync(_apiPrefix + "/identity/login", loginPayload, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("[ElsaPublishAdapter] Login failed {Status} {Body}",
                (int)response.StatusCode, body);
            return null;
        }

        var result = await response.Content.ReadFromJsonAsync<ElsaLoginResponse>(cancellationToken: ct);
        return result?.AccessToken;
    }
}