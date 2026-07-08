using Elsa.Common.Models;
using Elsa.Workflows.Runtime.Parameters;

namespace EnterpriseWorkflow.Elsa.Adapters
{
    internal class StartWorkflowRuntimeOptions : StartWorkflowRuntimeParams
    {
        public VersionOptions VersionOptions { get; set; }
        public Dictionary<string, object> Input { get; set; }
        public string CorrelationId { get; set; }
        public string TenantId { get; set; }
    }
}