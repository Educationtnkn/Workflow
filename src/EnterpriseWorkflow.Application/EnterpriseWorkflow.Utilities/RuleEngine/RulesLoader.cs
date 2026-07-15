using System.Text.Json;
using RulesEngine.Models;

namespace AppFW.Utilities.RuleEngine
{
    public static class RuleLoader
    {
        public static Workflow[] Load(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new RulesConfigurationException("Rules path is required.");
            }

            if (!File.Exists(path))
            {
                throw new RulesConfigurationException($"Rules file not found: {path}");
            }

            var json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                throw new RulesConfigurationException($"Rules file is empty: {path}");
            }

            try
            {
                var workflows = JsonSerializer.Deserialize<Workflow[]>(json);
                if (workflows is null || workflows.Length == 0)
                {
                    throw new RulesConfigurationException($"Rules file has no workflows: {path}");
                }

                return workflows;
            }
            catch (JsonException ex)
            {
                throw new RulesConfigurationException($"Invalid rules JSON format in file: {path}. {ex.Message}");
            }
        }
    }
}
