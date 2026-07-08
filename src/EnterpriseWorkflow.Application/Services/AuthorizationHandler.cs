using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Enums;

using EnterpriseWorkflow.Domain.ValueObjects;
using EnterpriseWorkflow.Utilities.Observability;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Traces;

namespace EnterpriseWorkflow.Application.Services;
public class AuthorizationHandler : IAuthorizationHandler
{
    private readonly ISecurityPolicyStore _policyStore;
    private readonly ILoggingService _logger;
    private readonly INDTracerService _tracer;

    public AuthorizationHandler(ISecurityPolicyStore policyStore,
        INDTracerService tracer,
        ILoggingService logger)
    {
        _policyStore = policyStore;
        _tracer = tracer;
        _logger = logger;
    }

    public async Task AuthorizeAsync(ExecutionModel ctx,string permission, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "HasPermission",
            "Checking Permission Started");
        // Simple RBAC check – in real life you would evaluate roles/claims
        var allowed = await _policyStore.HasPermissionAsync(ctx.TenantId, ctx.DomainId, ctx.UserId, permission, ct);
        _logger.LogInformation(
            "HasPermission",
            "Checking Permission Completed");
        if (!allowed)
            throw new UnauthorizedAccessException($"Permission '{permission}' denied for user {ctx.UserId}");
    }

}


