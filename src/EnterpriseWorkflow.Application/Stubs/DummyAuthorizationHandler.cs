using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.ValueObjects;

namespace EnterpriseWorkflow.Application.Stubs;

public class DummyAuthorizationHandler : IAuthorizationHandler
{
    public Task AuthorizeAsync(ExecutionModel ctx, string permission, CancellationToken ct = default)
    {
        // Dummy: always allow
        return Task.CompletedTask;
    }
}