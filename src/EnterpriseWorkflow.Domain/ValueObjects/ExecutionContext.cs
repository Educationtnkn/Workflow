namespace EnterpriseWorkflow.Domain.ValueObjects;

public sealed record ExecutionModel
{
    public required string TenantId { get; init; }
    public string DomainId { get; init; } = string.Empty;
    public required string UserId { get; init; }
    public string CorrelationId { get; init; } = Guid.NewGuid().ToString();
    public System.Security.Claims.ClaimsPrincipal? Principal { get; init; }
}
