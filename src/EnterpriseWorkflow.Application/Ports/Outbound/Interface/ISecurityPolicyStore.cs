using EnterpriseWorkflow.Domain.ValueObjects;

namespace EnterpriseWorkflow.Application.Ports.Outbound.Interface;

public interface ISecurityPolicyStore
{
    Task<bool> HasPermissionAsync(string tenantId, string domainId, string userId, string permission, CancellationToken ct = default);
}