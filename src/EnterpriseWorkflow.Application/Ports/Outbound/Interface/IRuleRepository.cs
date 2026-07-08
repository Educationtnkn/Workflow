using EnterpriseWorkflow.Domain.Entities.RuleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Outbound.Interface
{
    public interface IRuleRepository
    {
        /// <summary>Fetch a RuleSet by its logical workflow name (matches EvaluateRuleActivity's ruleWorkflow input).</summary>
        Task<RuleSetDefinition?> GetByWorkflowNameAsync(string workflowName, CancellationToken ct = default);

        /// <summary>Fetch a RuleSet by its numeric ID.</summary>
        Task<RuleSetDefinition?> GetByIdAsync(int ruleSetId, CancellationToken ct = default);

        Task<IReadOnlyList<RuleSetDefinition>> GetAllAsync(CancellationToken ct = default);
    }
}
