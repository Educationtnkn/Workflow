namespace EnterpriseWorkflow.Domain.ValueObjects;

public readonly record struct TenantDomainKey(string TenantId, string DomainId)
{
    public override string ToString() => $"{TenantId}:{DomainId}";
    public string ToCacheKey(string prefix) => $"{prefix}:{TenantId}:{DomainId}";
}
