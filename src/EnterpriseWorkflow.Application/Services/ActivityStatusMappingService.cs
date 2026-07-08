using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Utilities.Observability;
using Microsoft.Extensions.Configuration;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Traces;
using System.Text.Json;
public interface IActivityStatusMappingService
{

    string? GetBusinessMessage(
        string workflowDefinitionId,
        string activityId,
        string activityStatus);
}

public class ActivityStatusMappingService : IActivityStatusMappingService
{
    private readonly Dictionary<string, string> _mappings;
    private readonly ActionStatusConfiguration _configuration;
    private readonly ILoggingService _logger;
    private readonly INDTracerService _tracer;
    public ActivityStatusMappingService(IConfiguration configuration,
        INDTracerService tracer,
        ILoggingService logger)

    {
        _tracer = tracer;
        _logger = logger;
        // reads from activityStatusMapping.json 
        var mappingPath = configuration["ActivityStatusMappingPath"]
                          ?? Path.Combine("Stubs", "ActivityStatusMapping.json");

        var json = File.ReadAllText(mappingPath);

        _configuration = JsonSerializer.Deserialize<ActionStatusConfiguration>(
            json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })
            ?? throw new InvalidOperationException(
                "Unable to deserialize ActivityStatusMapping.json");
    }

    public string GetBusinessMessage(
        string workflowDefinitionId,
        string activityId,
        string status)
    {
        _logger.LogInformation(
       "ActivityStatusMapping",
       "[Service] ActivityStatusMapping Started");
        // Ensure it is the correct workflow definition.
        if (!_configuration.WorkflowDefinitionId.Equals(
                workflowDefinitionId,
                StringComparison.OrdinalIgnoreCase))
        {
            return $"No mapping found for workflow '{workflowDefinitionId}'.";
        }

        var activity = _configuration.ActionStatusMappings
            .FirstOrDefault(x =>
                x.ActionId.Equals(
                    activityId,
                    StringComparison.OrdinalIgnoreCase));

        if (activity == null)
        {
            return $"No mapping found for activity '{activityId}'.";
        }

        if (activity.StatusMappings.TryGetValue(status, out var message))
        {
            return message;
        }
        _logger.LogInformation(
      "ActivityStatusMapping",
      "[Service] ActivityStatusMapping Completed");
        return $"No business message configured for status '{status}'.";
    }
}