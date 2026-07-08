using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Entities.RuleEngine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Storage.Implementations.Repositories
{
    public class JsonRuleRepositoryOptions
    {
        /// <summary>Absolute or relative path to RuleDefinitions.json</summary>
        public string FilePath { get; set; } = "RuleStore/RuleDefinitions.json";
    }

    public class JsonRuleRepository : IRuleRepository
    {
        private readonly ILogger<JsonRuleRepository> _logger;
        private readonly string _filePath;

        // Lazy-loaded, cached in memory after first read
        private IReadOnlyList<RuleSetDefinition>? _cache;
        private readonly SemaphoreSlim _lock = new(1, 1);

        private static readonly JsonSerializerOptions _opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public JsonRuleRepository(
            IOptions<JsonRuleRepositoryOptions> options,
            ILogger<JsonRuleRepository> logger)
        {
            _filePath = options.Value.FilePath;
            _logger = logger;
        }

        public async Task<RuleSetDefinition?> GetByWorkflowNameAsync(
            string workflowName, CancellationToken ct = default)
        {
            var all = await LoadAsync(ct);
            return all.FirstOrDefault(r =>
                string.Equals(r.WorkflowName, workflowName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<RuleSetDefinition?> GetByIdAsync(int ruleSetId, CancellationToken ct = default)
        {
            var all = await LoadAsync(ct);
            return all.FirstOrDefault(r => r.RuleSetId == ruleSetId);
        }

        public async Task<IReadOnlyList<RuleSetDefinition>> GetAllAsync(CancellationToken ct = default)
            => await LoadAsync(ct);

        private async Task<IReadOnlyList<RuleSetDefinition>> LoadAsync(CancellationToken ct)
        {
            if (_cache is not null) return _cache;

            await _lock.WaitAsync(ct);
            try
            {
                if (_cache is not null) return _cache;

                var path = Path.IsPathRooted(_filePath)
                    ? _filePath
                    : Path.Combine(AppContext.BaseDirectory, _filePath);

                if (!File.Exists(path))
                    throw new FileNotFoundException($"Rule definitions file not found: {path}");

                var json = await File.ReadAllTextAsync(path, ct);
                var ruleSets = JsonSerializer.Deserialize<List<RuleSetDefinition>>(json, _opts)
                    ?? throw new InvalidOperationException("Rule definitions file is empty or invalid.");

                _logger.LogInformation("[RuleRepo] Loaded {Count} rule sets from {Path}",
                    ruleSets.Count, path);

                _cache = ruleSets.AsReadOnly();
                return _cache;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
