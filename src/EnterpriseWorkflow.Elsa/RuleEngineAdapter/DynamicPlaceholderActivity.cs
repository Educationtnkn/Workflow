
using Elsa.Extensions;
using Elsa.Workflows;
using Elsa.Workflows.Attributes;
using EnterpriseWorkflow.Application.Ports.Inbound.Interface;
using EnterpriseWorkflow.Domain.ValueObjects;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AppFW.Utilities.RuleEngine;

[Activity(
    "Custom",
    "DynamicPlaceholder",
    "Builds and executes a dynamic child workflow from HTTP input")]
public class DynamicPlaceholderActivityV2 : CodeActivity
{
    protected override async ValueTask ExecuteAsync(ActivityExecutionContext context)
    {
        var logger = context.GetRequiredService<ILogger<DynamicPlaceholderActivityV2>>();
        var workflowExecutionBuilder = context.GetRequiredService<IWorkflowExecution>();
        var workflowDefinitionBuilder = context.GetRequiredService<IWorkflowDefinition>();

        try
        {
            // Read request body captured by HttpEndpoint.
            var body = context.GetVariable<object>("HttpRequestBody");

            logger.LogInformation(
                "HttpRequestBody Type = {Type}",
                body?.GetType().FullName);

            logger.LogInformation(
                "HttpRequestBody Value = {Value}",
                JsonSerializer.Serialize(body));

            if (body == null)
            {
                logger.LogWarning("[Dynamic] Request body is null");
                await context.CompleteActivityAsync();
                return;
            }
            var filePath =
    Path.GetFullPath(
        Path.Combine(
            AppContext.BaseDirectory,
            @"..\..\..\..\EnterpriseWorkflow.Application\Stubs\Activities.json"));

            var activityCatalogJson =
                await File.ReadAllTextAsync(filePath);

            var activityCatalog =
                JsonSerializer.Deserialize<List<ActivityCatalogItem>>(
                    activityCatalogJson)!;
            // Convert to strongly typed request.
            var json = JsonSerializer.Serialize(body);

            var request = JsonSerializer.Deserialize<DynamicActivityRequest>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (request?.Activities == null || !request.Activities.Any())
            {
                logger.LogWarning("[Dynamic] No activities supplied");
                await context.CompleteActivityAsync();
                return;
            }

            logger.LogInformation(
                "[Dynamic] Building workflow from {Count} activities",
                request.Activities.Count);
            var selectedActivities =
    request.Activities
        .Select(name =>
        {
            var match = activityCatalog.FirstOrDefault(x =>
                string.Equals(
                    x.Name?.Trim(),
                    name?.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            logger.LogInformation(
                "Lookup '{Name}' => {Result}",
                name,
                match?.Name ?? "NOT FOUND");

            return match;
        })
        .Where(x => x != null)
        .ToList();


            var root =
                BuildFlowchart(selectedActivities);

            var workflowDefinition = new
            {
                model = new
                {
                    DefinitionId = $"Dynamic_{Guid.NewGuid():N}",
                    Name = "Dynamic Child Workflow",
                    IsLatest = true,
                    IsPublished = true,
                    Root = root
                }
            };

            var workflowJson =
                JsonSerializer.Serialize(
                    workflowDefinition,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
            // ------------------------------------------------------------------
            // Step 1 - Create workflow definition.
            // ------------------------------------------------------------------
            var definitionId =
                await workflowDefinitionBuilder.CreateDefinitionAsync(
                    workflowJson);

            logger.LogInformation(
                "[Dynamic] Child workflow definition created: {DefinitionId}",
                definitionId);
            var parentWorkflowInstanceId = context.WorkflowExecutionContext.Id;
            // ------------------------------------------------------------------
            // Step 2 - Run child workflow.
            // ------------------------------------------------------------------

    //        var executeResponse =
    //await workflowExecutionBuilder.ExecuteDefinitionWithParentIdAsync(definitionId, parentWorkflowInstanceId);

    //        var workflowInstanceId = executeResponse.ExecutionId;

            //logger.LogInformation(
            //    "[Dynamic] Child workflow started: {WorkflowInstanceId}",
            //    workflowInstanceId);

            // ------------------------------------------------------------------
            // Step 3 - Wait for child workflow completion.
            // ------------------------------------------------------------------
            //await workflowExecutionBuilder.WaitForCompletionAsync(
            //    workflowInstanceId.ToString());

            //logger.LogInformation(
            //    "[Dynamic] Child workflow completed: {WorkflowInstanceId}",
            //    workflowInstanceId);

            // ------------------------------------------------------------------
            // Optional: expose child workflow information to parent workflow.
            // ------------------------------------------------------------------
            context.SetVariable(
                "ChildWorkflowDefinitionId",
                definitionId);

            //context.SetVariable(
            //    "ChildWorkflowInstanceId",
            //    workflowInstanceId);

            // ------------------------------------------------------------------
            // Complete current activity.
            // Parent workflow continues to next activity.
            // ------------------------------------------------------------------
            await context.CompleteActivityAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "[Dynamic] Failed to execute dynamic workflow");

            throw;
        }
    }

    private object BuildFlowchart(
        List<ActivityCatalogItem> activities)
    {
        var activityNodes = new List<Dictionary<string, object>>();
        var connections = new List<object>();

        string? previousId = null;

        foreach (var activity in activities)
        {
            var activityId =
                $"{activity.Name}_{Guid.NewGuid():N}";

            var node =
                BuildActivityNode(
                    activityId,
                    activity);

            activityNodes.Add(node);

            if (previousId != null)
            {
                connections.Add(new
                {
                    source = new
                    {
                        activity = previousId,
                        port = "Done"
                    },
                    target = new
                    {
                        activity = activityId,
                        port = "In"
                    }
                });
            }

            previousId = activityId;
        }

        return new
        {
            id = "Flowchart1",
            type = "Elsa.Flowchart",
            activities = activityNodes,
            connections = connections
        };
    }

    private Dictionary<string, object> BuildActivityNode(
    string activityId,
    ActivityCatalogItem activity)
    {
        switch (activity.Name)
        {
            case "WriteLine":

                activity.Props.TryGetValue(
                    "text",
                    out var text);

                return new Dictionary<string, object>
                {
                    ["id"] = activityId,
                    ["type"] = "Elsa.WriteLine",
                    ["text"] = new
                    {
                        expression = new
                        {
                            type = "Literal",
                            value = text?.ToString()
                        }
                    }
                };

            case "Delay":
                activity.Props.TryGetValue(
                    "seconds",
                    out var seconds);
                return new Dictionary<string, object>
                {
                    ["id"] = activityId,
                    ["type"] = "Elsa.Delay",
                    ["timeSpan"] = new
                    {
                        typeName = "TimeSpan",
                        expression = new
                        {
                            type = "Literal",
                            value = seconds?.ToString()
                        }
                    }
                };

            case "RunJavaScript":
                activity.Props.TryGetValue(
                    "script",
                    out var script);

                return new Dictionary<string, object>
                {
                    ["id"] = activityId,
                    ["type"] = "Elsa.RunJavaScript",
                    ["script"] = new
                    {
                        expression = new
                        {
                            type = "JavaScript",
                            value = script?.ToString()
                        }
                    }
                };

            case "SetVariable":
                activity.Props.TryGetValue(
                    "variableName",
                    out var variableName);

                activity.Props.TryGetValue(
                    "value",
                    out var value);

                return new Dictionary<string, object>
                {
                    ["id"] = activityId,
                    ["type"] = "Elsa.SetVariable",
                    ["variable"] = new
                    {
                        name = variableName?.ToString()
                    },
                    ["value"] = new
                    {
                        expression = new
                        {
                            type = "Literal",
                            value = value?.ToString()
                        }
                    }
                };

            default:
                throw new InvalidOperationException(
                    $"Unsupported activity '{activity.Name}'.");
        }
    }
}
public class DynamicActivityRequest
{
    public List<string> Activities { get; set; } = new();
}

public class ActivityDefinition
{
    public string Type { get; set; } = default!;
    public Dictionary<string, object> Props { get; set; } = new();
}
public class ActivityCatalogItem
{
    public string ActivityId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public Dictionary<string, object> Props { get; set; } = new();
}

