using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Application.UseCases.CreateDefinition
{
    // CreateWorkflowDefinitionHandler.cs
    public class CreateWorkflowDefinitionHandler
    {
        private readonly IWorkflowDefinitionRepository _repo;
        private readonly IAuthorizationHandler _auth;
        //private readonly IExecutionLogger _logger;

        public CreateWorkflowDefinitionHandler(
            IWorkflowDefinitionRepository repo,
            IAuthorizationHandler auth
           //, IExecutionLogger logger
            )
        {
            _repo = repo;
            _auth = auth;
          //  _logger = logger;
        }

        public async Task<string> HandleAsync(WorkflowdefinitionDto command, CancellationToken ct)
        {
           // // 1. Authorize
           //// objectvalue = await _auth.AuthorizeAsync(command.CallerContext, Permission.ManageDefinition, ct);

           // // 2. Check if definitionId already exists
           // if (await _repo.ExistsAsync(command.DefinitionId, ct))
           //     throw new InvalidOperationException($"DefinitionId '{command.DefinitionId}' already exists.");

           // // 3. Map DTOs to domain entities
           // var definition = WorkflowDefinitionMapper.ToEntity(command);

           // // 4. Persist to enterprise DB (draft)
           // var id = await _repo.SaveAsync(definition, ct);

            // 5. Log
           // await _logger.LogAsync(id, LogType.Info, $"Workflow definition '{definition.Name}' created (draft).", command.CallerContext, ct);

            //return id;
            return "";
        }
    }
}
