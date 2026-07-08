using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.ValueObjects;

namespace EnterpriseWorkflow.Application.Stubs;

public class DummySecurityPolicyStore : ISecurityPolicyStore
{
    public Task<bool> HasPermissionAsync(string tenantId, string domainId, string userId, string permission, CancellationToken ct = default)
    {
        // Dummy: always allow
        return Task.FromResult(true);
    }
}