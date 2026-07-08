using EnterpriseWorkflow.Application.Ports.Inbound.Interface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.ValueObjects;
using EnterpriseWorkflow.Utilities.Observability;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Traces;

namespace EnterpriseWorkflow.Api.Stubs;

public class DummySecurityPolicyStore : ISecurityPolicyStore
{
    private readonly ILoggingService _logger;
    private readonly INDTracerService _tracer;

    public DummySecurityPolicyStore(
         INDTracerService tracer,
        ILoggingService logger)
    {
        _tracer = tracer;
        _logger = logger;
    }
    public Task<bool> HasPermissionAsync(string tenantId, string domainId, string userId, string permission, CancellationToken ct = default)
    {
        //_logger.LogInformation(
        //    "HasPermission",
        //    "Checking Permission Started");
        //_logger.LogInformation(
        //    "HasPermission",
        //    "Checking Permission Completed");
        // Dummy: always allow
        return Task.FromResult(true);
    }
}