
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Enums;
using EnterpriseWorkflow.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.UseCases.PublishDefinition
{
    // PublishWorkflowDefinitionHandler.cs
    public class PublishWorkflowDefinitionHandler
    {
        private readonly IWorkflowDefinitionRepository _repo;
        private readonly IWorkflowEngineAdapter _engineAdapter;
        private readonly IAuthorizationHandler _auth;
       // private readonly IExecutionLogger _logger;

        public PublishWorkflowDefinitionHandler(
            IWorkflowDefinitionRepository repo,
            IWorkflowEngineAdapter engineAdapter,
            IAuthorizationHandler auth
            //,IExecutionLogger logger
            )
        {
            _repo = repo;
            _engineAdapter = engineAdapter;
            _auth = auth;
           // _logger = logger;
        }

        public async Task HandleAsync(PublishWorkflowDefinitionCommand command, CancellationToken ct)
        {
            // 1. Load definition from enterprise DB
            //var definition = await _repo.GetByDefinitionIdAndVersionAsync(command.DefinitionId, command.Version, ct);
          //  if (definition == null)
          //      throw new WorkflowNotFoundException($"Definition {command.DefinitionId} v{command.Version} not found.");

          //  // 2. Authorize
          // // await _auth.AuthorizeAsync(command.CallerContext, Permission.ManageDefinition, ct);

          //  // 3. Validate with adapter (Elsa compatibility)
          //  var validation = await _engineAdapter.ValidateDefinitionAsync(definition, ct);
          //  if (!validation.IsValid)
          //      throw new WorkflowValidationException(string.Join(", ", validation.Errors.Select(e => e.Message)));

          //  // 4. Register definition with Elsa (adapter converts enterprise model to Elsa JSON)
          //  var elsaDefinitionId = await _engineAdapter.RegisterDefinitionAsync(definition, command.CallerContext, ct);

          //  // 5. Update enterprise record as published
          ////  definition.IsPublished = true;
          //  //definition.ElsaDefinitionId = elsaDefinitionId;
          //  await _repo.UpdateAsync(definition, ct);

            // 6. Log success
          //  await _logger.LogAsync(definition.Id, LogType.Info,   $"Workflow published to Elsa (ElsaDefinitionId: {elsaDefinitionId})", command.CallerContext, ct);
        }
    }
}
