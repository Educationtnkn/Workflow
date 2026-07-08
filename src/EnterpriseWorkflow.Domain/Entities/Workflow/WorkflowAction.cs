using System.Text.Json;

namespace EnterpriseWorkflow.Domain.Entities.Workflow;


public class WorkflowAction
{
    //public Configurations.RunJavaScriptConfig Config;

    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? TaskId { get; set; } = default!;
    public string? ActionName { get; set; } = default!;
    public string ActionType { get; set; } = default!;
    public string? ActionCategory { get; set; } = default!;
    public int SeqNo { get; set; }
    public bool? IsConditional { get; set; }
    public string? RuleSetId { get; set; }
    public bool? IsDynamic { get; set; }
    public string? DynamicRegistrationId { get; set; }
    public bool? HasConnection { get; set; } = true;

    // Configuration as JSON string
    public string? ConfigJson { get; set; } = "{}";

    public static WorkflowAction Create(string actionName, string actionType, int seqNo, object config)
    {
        return new WorkflowAction
        {
            ActionName = actionName,
            ActionType = actionType,
            SeqNo = seqNo,
            ConfigJson = JsonSerializer.Serialize(config)

        };
    }

    public T GetConfig<T>() where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(ConfigJson))
            return new T();

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<T>(ConfigJson, options) ?? new T();
        }
        catch (JsonException)
        {
            return new T();
        }
    }

    public void UpdateConfig(object config)
    {
        ConfigJson = JsonSerializer.Serialize(config);
    }

   
}