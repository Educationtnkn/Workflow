using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Elsa.Models
{
    public class ElsaWorkflowDefinition
    {
        public string DefinitionId { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public ElsaWorkflowOptions Options { get; set; } = new();
        public List<ElsaVariable> Variables { get; set; } = new();
        public List<object> Inputs { get; set; } = new();
        public List<object> Outputs { get; set; } = new();
        public List<object> Outcomes { get; set; } = new();
        public Dictionary<string, object> CustomProperties { get; set; } = new();
        public ElsaFlowchartRoot Root { get; set; } = default!;
    }

    public class ElsaWorkflowOptions
    {
        public string? ActivationStrategyType { get; set; }
        public bool? UsableAsActivity { get; set; }
        public bool AutoUpdateConsumingWorkflows { get; set; } = false;
        public string? ActivityCategory { get; set; }
    }

    public class ElsaVariable
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string TypeName { get; set; } = "String";
        public bool IsArray { get; set; } = false;
        public string? Value { get; set; }
        public string StorageDriverTypeName { get; set; } =
            "Elsa.Workflows.WorkflowInstanceStorageDriver, Elsa.Workflows.Core";
    }

    public class ElsaFlowchartRoot
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = default!;

        [JsonPropertyName("nodeId")]
        public string NodeId { get; set; } = default!;

        [JsonPropertyName("type")]
        public string Type { get; set; } = "Elsa.Flowchart";

        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("customProperties")]
        public Dictionary<string, object> CustomProperties { get; set; } = new()
        {
            ["notFoundConnections"] = new List<object>(),
            ["canStartWorkflow"] = false,
            ["runAsynchronously"] = false
        };

        [JsonPropertyName("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new();

        [JsonPropertyName("activities")]
        public List<Dictionary<string, object>> Activities { get; set; } = new();

        [JsonPropertyName("variables")]
        public List<object> Variables { get; set; } = new();

        [JsonPropertyName("connections")]
        public List<ElsaConnection> Connections { get; set; } = new();
    }

    public class ElsaConnection
    {
        [JsonPropertyName("source")]
        public ElsaConnectionEndpoint Source { get; set; } = default!;

        [JsonPropertyName("target")]
        public ElsaConnectionEndpoint Target { get; set; } = default!;

        [JsonPropertyName("vertices")]
        public List<object> Vertices { get; set; } = new();
    }

    public class ElsaConnectionEndpoint
    {
        [JsonPropertyName("activity")]
        public string Activity { get; set; } = default!;

        [JsonPropertyName("port")]
        public string Port { get; set; } = default!;
    }
}
