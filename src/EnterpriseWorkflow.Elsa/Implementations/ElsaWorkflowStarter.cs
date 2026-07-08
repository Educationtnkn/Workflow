using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Parameters;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Elsa.Adapters;
using EnterpriseWorkflow.Storage.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Elsa.Implementations
{

    public class ElsaWorkflowStarter : IElsaWorkflowStarter
    {
        private readonly IWorkflowRuntime _runtime;

        public ElsaWorkflowStarter(IWorkflowRuntime runtime) => _runtime = runtime;

        public async Task<ElsaStartResult> StartAsync(
            string workflowDefinitionId,
            string correlationId,
            IDictionary<string, object> input,
            CancellationToken ct)
        {
            try
            {
                var options = new StartWorkflowRuntimeParams
                {
                    CorrelationId = correlationId,
                    Input = new Dictionary<string, object>(input)
                };

                var result = await _runtime.StartWorkflowAsync(workflowDefinitionId, options);

                return new ElsaStartResult
                {
                    Success = true,
                    ElsaWorkflowInstanceId = result.WorkflowInstanceId,
                    Status = result.Status.ToString()
                };
            }
            catch (Exception ex)
            {
                return new ElsaStartResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ElsaResumeResult> ResumeAsync(
            string elsaWorkflowInstanceId,
            string bookmarkId,
            IDictionary<string, object> input,
            CancellationToken ct)
        {
            try
            {
                var options = new ResumeWorkflowRuntimeParams
                {
                    BookmarkId = bookmarkId,
                    Input = new Dictionary<string, object>(input)
                };

                var result = await _runtime.ResumeWorkflowAsync(elsaWorkflowInstanceId, options);

                if (result is null)
                {
                    return new ElsaResumeResult
                    {
                        Success = false,
                        ErrorMessage = $"No matching bookmark/instance found for {elsaWorkflowInstanceId}."
                    };
                }

                return new ElsaResumeResult
                {
                    Success = true,
                    Status = result.Status.ToString()
                };
            }
            catch (Exception ex)
            {
                return new ElsaResumeResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> TryCancelAsync(string elsaWorkflowInstanceId, CancellationToken ct)
        {
            try
            {
                await _runtime.CancelWorkflowAsync(elsaWorkflowInstanceId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
    
}
