using EnterpriseWorkflow.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.Ports.Outbound.Interface;

public interface IAuthorizationHandler
{
    Task AuthorizeAsync(ExecutionModel ctx, string permission, CancellationToken ct = default);
}

