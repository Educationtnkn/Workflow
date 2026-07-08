using Elsa.Common.Models;
using Elsa.Expressions.Helpers;
using Elsa.Scheduling.Activities;
using Elsa.Workflows;
using Elsa.Workflows.Management;
using Elsa.Workflows.Management.Contracts;
using Elsa.Workflows.Management.Entities;
using Elsa.Workflows.Management.Filters;
using Elsa.Workflows.Management.Models;
using Elsa.Workflows.Models;
using Elsa.Workflows.Runtime;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Contracts;
using Elsa.Workflows.Runtime.Entities;
using Elsa.Workflows.Runtime.Filters;
using Elsa.Workflows.Runtime.Messages;
using Elsa.Workflows.Runtime.Requests;
using Elsa.Workflows.Runtime.Results;
using EnterpriseWorkflow.Application.Model;
using EnterpriseWorkflow.Application.Ports.Outbound.AdapterInterface;
using EnterpriseWorkflow.Application.Ports.Outbound.Interface;
using EnterpriseWorkflow.Domain.Configurations;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.Enums;
using EnterpriseWorkflow.Domain.Exceptions;
using EnterpriseWorkflow.Domain.ValueObjects;
using EnterpriseWorkflow.Elsa.Mapping;
using Microsoft.Extensions.Logging;
using Quartz.Util;
using System.Text.Json;
using static Google.Apis.Storage.v1.ObjectsResource;
using DomainStatus = EnterpriseWorkflow.Domain.Enums.WorkflowStatus;
using ElsaStatus = Elsa.Workflows.WorkflowStatus;
using ElsaSubStatus = Elsa.Workflows.WorkflowSubStatus;

namespace EnterpriseWorkflow.Elsa.Adapters;

    public sealed class ElsaWorkflowAdapter : IWorkflowEngineAdapter
    {
    public string EngineType => "Elsa";

    private readonly IWorkflowDefinitionStore _definitionStore;
    private readonly IWorkflowInstanceStore _instanceStore;
    private readonly IWorkflowRuntime _workflowRuntime;
    private readonly IBookmarkStore _bookmarkStore;
    private readonly EnterpriseToElsaMapper _mapper;
    private readonly ILogger<ElsaWorkflowAdapter> _logger;

    private readonly IWorkflowDispatcher _workflowDefinitionDispatcher;
    private readonly IWorkflowDefinitionImporter _workflowDefinitionImporter;
    private readonly IApiSerializer _apiSerializer;
    private readonly IWorkflowResumer _workflowResumer;
    private readonly IActivityExecutionStore _activityExecutionStore;


    public ElsaWorkflowAdapter(
        IWorkflowDefinitionStore definitionStore,
        IWorkflowInstanceStore instanceStore,
        IWorkflowRuntime workflowRuntime,
        IBookmarkStore bookmarkStore,
        EnterpriseToElsaMapper mapper,
        ILogger<ElsaWorkflowAdapter> logger,
        IWorkflowDefinitionImporter workflowDefinitionImporter, IApiSerializer apiSerializer,
        IWorkflowDispatcher workflowDefinitionDispatcher,

        IWorkflowResumer workflowResumer,
        IActivityExecutionStore activityExecutionStore
        )
    {
        _definitionStore = definitionStore;
        _instanceStore = instanceStore;
        _workflowRuntime = workflowRuntime;
        _bookmarkStore = bookmarkStore;
        _mapper = mapper;
        _logger = logger;
        _workflowDefinitionImporter = workflowDefinitionImporter;
        _workflowDefinitionDispatcher = workflowDefinitionDispatcher;
        _apiSerializer = apiSerializer;
        _workflowResumer = workflowResumer;
        _activityExecutionStore = activityExecutionStore;
    }

    public async Task<string> RegisterDefinitionAsync(
        Workflowdefinition definition,
        ExecutionModel ctx,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Registering workflow {DefinitionId} v{Version} with Elsa 3.1.3",
            definition.DefinitionId, definition.Version);

        // 1. Validate before conversion
        var validation = await ValidateDefinitionAsync(definition, cancellationToken);
        if (!validation.IsValid)
        {
            throw new WorkflowValidationException(
                $"Workflow validation failed: {string.Join(", ", validation.Errors.Select(e => e.Message))}");
        }

        // 2. Convert enterprise model to Elsa's JSON format

        _logger.LogInformation("[Mapper] Domain model to ELSA Model convertion started");
        var elsaWorkflowJson = _mapper.ConvertToElsaJson(definition);
        _logger.LogInformation("[Mapper] Domain model to ELSA Model convertion completed successfully");
        //Console.WriteLine("Elsa Json : " + elsaWorkflowJson);

        var saveRequest = _apiSerializer.Deserialize<SaveWorkflowDefinitionRequest>(elsaWorkflowJson);
        if (saveRequest?.Model == null)
        {
            throw new InvalidOperationException("Workflow file is invalid. Expected top-level 'model' object.");
        }

        var importResult = await _workflowDefinitionImporter.ImportAsync(saveRequest, cancellationToken);
        if (!importResult.Succeeded)
        {
            var errors = string.Join("; ", importResult.ValidationErrors.Select(x => x.Message));
            throw new InvalidOperationException($"Workflow import failed: {errors}");
        }
        // 3. Create Elsa WorkflowDefinition entity
        //var elsaDefinition = new WorkflowDefinition
        //{
        //    Id = Guid.NewGuid().ToString(),
        //    DefinitionId = definition.DefinitionId,
        //    Name = definition.Name,
        //    Description = definition.Description,
        //    Version = definition.Version,
        //    StringData = elsaWorkflowJson,  // Elsa stores workflow as JSON string
        //    IsPublished = definition.IsPublished,
        //    IsLatest = definition.IsPublished,
        //    CreatedAt = DateTimeOffset.UtcNow,
        //  //  ToolVersion = "3.1.3",
        //    MaterializerName = "Json",  // Default materializer
        //   // UsableAsActivity = false
        //};

        // 4. Check if definition already exists using Elsa's filter
        //var existingFilter = new WorkflowDefinitionFilter
        //{
        //    DefinitionId = definition.DefinitionId,
        //    //Version = definition.Version
        //};

        //var existing = await _definitionStore.FindAsync(existingFilter, cancellationToken);

        //if (existing != null)
        //{
        //    _logger.LogInformation("Updating existing Elsa definition {DefinitionId} v{Version}",
        //        definition.DefinitionId, definition.Version);

        //    // Update existing definition
        //    existing.Name = definition.Name;
        //    existing.Description = definition.Description;
        //    existing.StringData = elsaWorkflowJson;
        //    existing.IsPublished = (bool)definition.IsPublished;
        //    existing.IsLatest = (bool)definition.IsPublished;

        //    await _definitionStore.SaveAsync(existing, cancellationToken);
        //    return existing.Id;
        //}

        // 5. Save new definition
        //await _definitionStore.SaveAsync(elsaDefinition, cancellationToken);

        _logger.LogInformation("Successfully registered workflow with Elsa. ElsaDefinitionId: {ElsaId}",
            importResult.WorkflowDefinition.Id);

        return importResult.WorkflowDefinition.Id;
    }

    //public async Task UpdateDefinitionAsync(
    //    Workflowdefinition definition,
    //    ExecutionModel ctx,
    //    CancellationToken cancellationToken = default)
    //{
    //    await RegisterDefinitionAsync(definition, ctx, cancellationToken);
    //}

    //public async Task UnregisterDefinitionAsync(
    //    string definitionId,
    //    int version,
    //    ExecutionModel ctx,
    //    CancellationToken cancellationToken = default)
    //{
    //    // Use Elsa's filter to find and delete
    //    var filter = new WorkflowDefinitionFilter
    //    {
    //        DefinitionId = definitionId,
    //        Version = version
    //    };

    //    var deletedCount = await _definitionStore.DeleteAsync(filter, cancellationToken);

    //    _logger.LogInformation("Unregistered workflow {DefinitionId} v{Version} from Elsa. Deleted count: {Count}",
    //        definitionId, version, deletedCount);
    //}

    public async Task<WorkflowStartResult> StartWorkflowAsync(
        WorkflowExecution workflowExecuteRequest,
        string? version,
        ExecutionModel ctx,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "StartWorkflowAsync started — DefinitionId: {DefinitionId}, TenantId: {TenantId}, CorrelationId: {CorrelationId}",
            workflowExecuteRequest.Workflow_Execution_Number,
            ctx.TenantId,
            ctx.CorrelationId);

        try
        {
            // ── Null check ─────────────────────────────────────────
            if (workflowExecuteRequest == null)
                throw new ArgumentNullException(nameof(workflowExecuteRequest),
                    "WorkflowExecution request cannot be null");

            if (string.IsNullOrWhiteSpace(workflowExecuteRequest.Workflow_Execution_Number))
                throw new ArgumentException(
                    "WorkflowDefinitionId cannot be null or empty",
                    nameof(workflowExecuteRequest.Workflow_Execution_Number));

            // ── Prepare input ──────────────────────────────────────
            var workflowInput = new Dictionary<string, object>
            {
                ["Payload"] = workflowExecuteRequest.Workflow_Execution_ID,
                ["TenantId"] = ctx.TenantId,
                ["DomainId"] = ctx.DomainId,
                ["CorrelationId"] = ctx.CorrelationId,
                ["UserId"] = ctx.UserId
            };

            // ── Build workflow options ─────────────────────────────
            var startOptions = new StartWorkflowOptions
            {
                CorrelationId = ctx.CorrelationId,
                TenantId = ctx.TenantId
            };

            // ── Create Elsa client ─────────────────────────────────
            _logger.LogInformation(
                "Creating Elsa workflow client — DefinitionId: {DefinitionId}",
                workflowExecuteRequest.Workflow_Execution_Number);

            var client = await _workflowRuntime.CreateClientAsync(cancellationToken);

            if (client == null)
            {
                _logger.LogError(
                    "Failed to create Elsa workflow client — DefinitionId: {DefinitionId}",
                    workflowExecuteRequest.Workflow_Execution_Number);

                throw new WorkflowEngineException("Elsa",
                    $"Failed to create workflow client for {workflowExecuteRequest.Workflow_Execution_Number}");
            }

            // ── Start workflow ─────────────────────────────────────
            _logger.LogInformation(
                "Sending CreateAndRunInstanceAsync to Elsa — DefinitionId: {DefinitionId}",
                workflowExecuteRequest.Workflow_Execution_Number);
            var versionOptions = string.IsNullOrWhiteSpace(version)
    ? VersionOptions.Latest
    : int.TryParse(version, out var versionNumber)
        ? VersionOptions.SpecificVersion(versionNumber)
        : VersionOptions.Latest;
            var startResult = await client.CreateAndRunInstanceAsync(
                new CreateAndRunWorkflowInstanceRequest
                {
                    WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(
                        workflowExecuteRequest.Workflow_Execution_Number,
                        versionOptions),
                    Input = workflowInput
                }, cancellationToken);

            // ── Null result check ──────────────────────────────────
            if (startResult == null)
            {
                _logger.LogError(
                    "Elsa returned null result — DefinitionId: {DefinitionId}, TenantId: {TenantId}",
                    workflowExecuteRequest.Workflow_Execution_Number,
                    ctx.TenantId);

                throw new WorkflowEngineException("Elsa",
                    $"Failed to start workflow {workflowExecuteRequest.Workflow_Execution_Number}");
            }

            _logger.LogInformation(
                "Workflow started successfully — EngineExecutionId: {EngineExecutionId}, Status: {Status}, SubStatus: {SubStatus}",
                startResult.WorkflowInstanceId,
                startResult.Status,
                startResult.SubStatus);

            // ── Map status ─────────────────────────────────────────
            var status = MapElsaStatusToExecutionStatus(startResult.Status);
            var subStatus = MapElsaSubStatusToExecutionStatus(startResult.SubStatus);
            var mappedBookmarks = startResult.Bookmarks?
                .Select(bookmark => new BookmarkResponseDto
                {
                    BookmarkId = bookmark.Id,
                    ActionName = ExtractEventName(bookmark.Payload),
                    ActionId = bookmark.ActivityId
                }).ToList() ?? new List<BookmarkResponseDto>();

            _logger.LogInformation(
                "Status mapped — ElsaStatus: {ElsaStatus}, MappedStatus: {MappedStatus}, MappedSubStatus: {MappedSubStatus}",
                startResult.Status,
                status,
                subStatus);

            return new WorkflowStartResult
            {
                ExecutionId = startResult.WorkflowInstanceId,
                EngineExecutionId = startResult.WorkflowInstanceId,
                Status = status,
                SubStatus = subStatus,
                Bookmark = mappedBookmarks
            };
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogError(ex,
                "Invalid argument in StartWorkflowAsync — DefinitionId: {DefinitionId}, Error: {Error}",
                workflowExecuteRequest?.Workflow_Execution_Number,
                ex.Message);
            throw;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex,
                "Argument error in StartWorkflowAsync — DefinitionId: {DefinitionId}, Error: {Error}",
                workflowExecuteRequest?.Workflow_Execution_Number,
                ex.Message);
            throw;
        }
        catch (WorkflowEngineException ex)
        {
            _logger.LogError(ex,
                "Workflow engine error in StartWorkflowAsync — DefinitionId: {DefinitionId}, Engine: {Engine}, Error: {Error}",
                workflowExecuteRequest?.Workflow_Execution_Number,
                ex.Message);
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex,
                "StartWorkflowAsync was cancelled — DefinitionId: {DefinitionId}",
                workflowExecuteRequest?.Workflow_Execution_Number);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in StartWorkflowAsync — DefinitionId: {DefinitionId}, TenantId: {TenantId}, Error: {Error}",
                workflowExecuteRequest?.Workflow_Execution_Number,
                ctx?.TenantId,
                ex.Message);
            throw;
        }
        finally
        {
            _logger.LogInformation(
                "StartWorkflowAsync completed — DefinitionId: {DefinitionId}",
                workflowExecuteRequest?.Workflow_Execution_Number);
        }
    }

    public async Task<DispatchWorkflowResponse> DispatchWorkflowAsync(WorkflowExecution workFlow, ExecutionModel ctx, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
           "DispatchWorkflowAsync started — DefinitionId: {DefinitionId}, TenantId: {TenantId}, CorrelationId: {CorrelationId}",
           workFlow.Workflow_Execution_Number,
           ctx.TenantId,
           ctx.CorrelationId);
        if (string.IsNullOrWhiteSpace(workFlow.Workflow_Execution_Number))
        {
            throw new ArgumentException("Definition ID is required.", nameof(workFlow.Workflow_Execution_Number));
        }

        var filter = new WorkflowDefinitionFilter { DefinitionId = workFlow.Workflow_Execution_Number };
        var definition = (await _definitionStore.FindManyAsync(filter))
            .OrderByDescending(x => x.Version)
            .FirstOrDefault();

        if (definition is null)
        {
            throw new KeyNotFoundException($"Workflow definition '{workFlow.Workflow_Execution_Number}' was not found.");
        }

        var instanceId = Guid.NewGuid().ToString("N");

        var input = new Dictionary<string, object>
        {
            ["Payload"] = workFlow.Workflow_Execution_ID,
            ["TenantId"] = ctx.TenantId,
            ["DomainId"] = ctx.DomainId,
            ["CorrelationId"] = ctx.CorrelationId,
            ["UserId"] = ctx.UserId
        };

        var request = new DispatchWorkflowDefinitionRequest(definition.Id)
        {
            InstanceId = instanceId,
            Input = input   // ✅ use dictionary
        };

        var result = await _workflowDefinitionDispatcher.DispatchAsync(request, new DispatchWorkflowOptions(), cancellationToken);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Failed to dispatch workflow definition.");
        }
        _logger.LogInformation(
           "DispatchWorkflowAsync Completed Successfully — DefinitionId: {DefinitionId}, TenantId: {TenantId}, CorrelationId: {CorrelationId}",
           workFlow.Workflow_Execution_Number,
           ctx.TenantId,
           ctx.CorrelationId);
        return new DispatchWorkflowResponse
        {
            WorkflowInstanceId = instanceId
        };
    }


    public async Task<string> CreateDefinitionAsync(
                string json,
                CancellationToken ct = default)
    {
        _logger.LogInformation(
           "CreateWorkflow Started");
        var saveRequest =
            _apiSerializer.Deserialize<SaveWorkflowDefinitionRequest>(json);

        if (saveRequest?.Model == null)
        {
            throw new InvalidOperationException(
                "Workflow file is invalid. Expected top-level 'model' object.");
        }

        var importResult =
            await _workflowDefinitionImporter.ImportAsync(
                saveRequest,
                ct);

        if (!importResult.Succeeded)
        {
            var errors = string.Join(
                "; ",
                importResult.ValidationErrors.Select(x => x.Message));

            throw new InvalidOperationException(
                $"Workflow import failed: {errors}");
        }
        _logger.LogInformation(
           "CreateWorkflow Completed - DefinitionId: {DefinitionId}", saveRequest.Model.DefinitionId);
        return saveRequest.Model.DefinitionId;
    }

    public async Task WaitForCompletionAsync(
        string workflowInstanceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
           "WaitForCompletion Started - ExecutionId: {ExecutionId}", workflowInstanceId);
        while (!cancellationToken.IsCancellationRequested)
        {
            var workflowInstance =
                await _instanceStore.FindAsync(
                    new WorkflowInstanceFilter { Id = workflowInstanceId },
                    cancellationToken: cancellationToken);


            if (workflowInstance == null)
            {
                throw new InvalidOperationException(
                    $"Workflow instance '{workflowInstanceId}' not found.");
            }

            var status = workflowInstance.Status;


            switch (status.ToString())
            {
                case "Finished":
                    return;

                case "Faulted":
                    throw new InvalidOperationException(
                        $"Workflow '{workflowInstanceId}' faulted.");

                case "Cancelled":
                    throw new InvalidOperationException(
                        $"Workflow '{workflowInstanceId}' was cancelled.");
            }

            await Task.Delay(
                TimeSpan.FromSeconds(1),
                cancellationToken);
        }
        _logger.LogInformation(
           "WaitForCompletion completed - ExecutionId: {ExecutionId}", workflowInstanceId);

        throw new OperationCanceledException(
            $"Waiting for workflow '{workflowInstanceId}' was cancelled.");
    }
    public async Task<WorkflowStartResult> ExecuteDefinitionWithParentIdAsync(string definitionId, string parentWorkflowId, Dictionary<string, object>? input = null, CancellationToken ct = default)
    {
        _logger.LogInformation(
          "ExecuteDefinitionWithParentIdAsync Started — DefinitionId: {DefinitionId}, ParentWorkflowId: {ParentWorkflowId}",
          definitionId,
          parentWorkflowId);
        if (string.IsNullOrWhiteSpace(definitionId))
        {
            throw new ArgumentException("Definition ID is required.", nameof(definitionId));
        }

        var client = await _workflowRuntime.CreateClientAsync(ct);
        var startResult = await client.CreateAndRunInstanceAsync(
            new CreateAndRunWorkflowInstanceRequest
            {
                WorkflowDefinitionHandle = WorkflowDefinitionHandle.ByDefinitionId(definitionId, VersionOptions.Latest),
                Input = input,
                ParentId = parentWorkflowId
            }, ct);

        var instanceId = startResult?.WorkflowInstanceId;
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            throw new InvalidOperationException("Workflow execution did not return an instance ID.");
        }

        var instance = await _instanceStore.FindAsync(
            new WorkflowInstanceFilter { Id = instanceId },
            cancellationToken: ct);

        if (instance?.WorkflowState == null)
        {
            throw new InvalidOperationException("Workflow execution did not return a workflow state.");
        }
        _logger.LogInformation(
         "ExecuteDefinitionWithParentIdAsync Completed successfully — DefinitionId: {DefinitionId}, ParentWorkflowId: {ParentWorkflowId}",
         definitionId,
         parentWorkflowId);
        return new WorkflowStartResult
        {
            ExecutionId = instance.WorkflowState.Id

        };
    }

    public async Task<string> LoadAndCreateAsync(string fileName,CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileName))
{
    throw new ArgumentException("File name is required.", nameof(fileName));
}

var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "Stubs", fileName);
if (!File.Exists(fullPath))
{
    throw new FileNotFoundException("Workflow file not found.", fullPath);
}

var json = await File.ReadAllTextAsync(fullPath, ct);

var saveRequest = _apiSerializer.Deserialize<SaveWorkflowDefinitionRequest>(json);
if (saveRequest?.Model == null)
{
    throw new InvalidOperationException("Workflow file is invalid. Expected top-level 'model' object.");
}

var importResult = await _workflowDefinitionImporter.ImportAsync(saveRequest, ct);

if (!importResult.Succeeded)
{
    var errors = string.Join("; ", importResult.ValidationErrors.Select(x => x.Message));
    throw new InvalidOperationException($"Workflow import failed: {errors}");
}

//var mapped = ElsaWorkflowModelMapper.ToDefinitionDetailsResponse(importResult.WorkflowDefinition);
return JsonSerializer.Serialize("");
    }


    //public async Task<ApproveWorkflowResponse> ApproveWorkflowAsync(
    //WorkflowAprroveRequest approveWorkflow,
    //ExecutionModel ctx,
    //CancellationToken cancellationToken = default)
    //{
    //    _logger.LogInformation(
    //        "ApproveWorkflowAsync started — ExecutionId: {DefinitionId}, TenantId: {TenantId}, CorrelationId: {CorrelationId}",
    //        approveWorkflow.workflowExecutionId,
    //        ctx.TenantId,
    //        ctx.CorrelationId);

    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(approveWorkflow.workflowExecutionId))
    //        {
    //            throw new ArgumentException(
    //                "Execution ID cannot be null or empty.",
    //                nameof(approveWorkflow.workflowExecutionId));
    //        }


    //        if (ctx == null)
    //        {
    //            throw new ArgumentNullException(nameof(ctx));
    //        }

    //        _logger.LogInformation(
    //            "Resuming workflow. ExecutionId: {ExecutionId}, Signal: {Signal}, UserId: {UserId}",
    //            approveWorkflow.workflowExecutionId,
    //            approveWorkflow.signal,
    //            ctx.UserId);

    //        var resumeInput = new Dictionary<string, object>
    //        {
    //            ["ResumePayload"] = approveWorkflow.payload ?? new { },
    //            ["ResumedBy"] = ctx.UserId ?? "System",
    //            ["ResumedAt"] = DateTimeOffset.UtcNow
    //        };

    //        var result = await ResumeHandleAsync(approveWorkflow.workflowExecutionId, approveWorkflow.signal, "approved", cancellationToken);
            
    //        _logger.LogInformation(
    //            "Workflow resumed successfully. ExecutionId: {ExecutionId}",
    //            approveWorkflow.workflowExecutionId);
    //        var response = new ApproveWorkflowResponse
    //        {
    //            WorkflowExecutionId = result.ExecutionId,
    //            Status = result.Status, 
    //            SubStatus = result.SubStatus,
    //            Approved = true
    //        };
    //        return response;
    //    }
    //    catch (ArgumentException ex)
    //    {
    //        _logger.LogWarning(
    //            ex,
    //            "Validation failed while resuming workflow. ExecutionId: {ExecutionId}",
    //            approveWorkflow.workflowExecutionId);

    //        throw;
    //    }
    //    catch (OperationCanceledException ex)
    //    {
    //        _logger.LogWarning(
    //            ex,
    //            "Workflow resume operation was cancelled. ExecutionId: {ExecutionId}",
    //            approveWorkflow.workflowExecutionId);

    //        throw;
    //    }
    //    catch (KeyNotFoundException ex)
    //    {
    //        _logger.LogWarning(
    //            ex,
    //            "Workflow or bookmark not found. ExecutionId: {ExecutionId}, Signal: {Signal}",
    //            approveWorkflow.workflowExecutionId,
    //            approveWorkflow.signal);

    //        throw;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(
    //            ex,
    //            "Unexpected error while resuming workflow. ExecutionId: {ExecutionId}, Signal: {Signal}",
    //            approveWorkflow.workflowExecutionId,
    //            approveWorkflow.signal);

    //        throw;
    //    }
    //}


    public async Task<CancelWorkflowResponse> CancelWorkflowAsync(
    WorkflowCancelRequest cancelWorkflow,
    ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "CancelWorkflowAsync started — ExecutionId: {ExecutionId}, TenantId: {TenantId}, CorrelationId: {CorrelationId}",
            cancelWorkflow.workflowExecutionId,
            ctx.TenantId,
            ctx.CorrelationId);

        try
        {
            if (string.IsNullOrWhiteSpace(cancelWorkflow.workflowExecutionId))
            {
                throw new ArgumentException(
                    "Execution ID cannot be null or empty.",
                    nameof(cancelWorkflow.workflowExecutionId));
            }


            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx));
            }

            _logger.LogInformation(
                "Resuming workflow. ExecutionId: {ExecutionId}, Signal: {Signal}, UserId: {UserId}",
                cancelWorkflow.workflowExecutionId,
                cancelWorkflow.signal,
                ctx.UserId);

            var resumeInput = new Dictionary<string, object>
            {
                ["ResumePayload"] = cancelWorkflow.payload ?? new { },
                ["ResumedBy"] = ctx.UserId ?? "System",
                ["ResumedAt"] = DateTimeOffset.UtcNow
            };

            var result = await ResumeHandleAsync(cancelWorkflow.workflowExecutionId, cancelWorkflow.signal, "rejected", cancellationToken);
            var response = new CancelWorkflowResponse
            {
                WorkflowExecutionId = result.ExecutionId,
                Status = result.Status,
                SubStatus = result.SubStatus,
                Cancelled = true
            };
            _logger.LogInformation(
                "Workflow resumed successfully. ExecutionId: {ExecutionId}",
                cancelWorkflow.workflowExecutionId);
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                ex,
                "Validation failed while resuming workflow. ExecutionId: {ExecutionId}",
                cancelWorkflow.workflowExecutionId);

            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(
                ex,
                "Workflow resume operation was cancelled. ExecutionId: {ExecutionId}",
                cancelWorkflow.workflowExecutionId);

            throw;
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(
                ex,
                "Workflow or bookmark not found. ExecutionId: {ExecutionId}, Signal: {Signal}",
                cancelWorkflow.workflowExecutionId,
                cancelWorkflow.signal);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while resuming workflow. ExecutionId: {ExecutionId}, Signal: {Signal}",
                cancelWorkflow.workflowExecutionId,
                cancelWorkflow.signal);

            throw;
        }
    }


    public async Task<ResumeWorkflowResponse> ResumeWorkflowAsync(
   WorkflowResumeRequest resumeWorkflow,
   ExecutionModel ctx,
   CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resumeWorkflow.workflowExecutionId))
        {
            throw new ArgumentException("Workflow ExecutionId is required", nameof(resumeWorkflow.workflowExecutionId));
        }

        if (string.IsNullOrWhiteSpace(resumeWorkflow.Task))
        {
            throw new ArgumentException("Task or Action is required", nameof(resumeWorkflow.Task));
        }

        // BookmarkFilter.Name == the Elsa.Event eventName
        var bookmarks = await _bookmarkStore.FindManyAsync(new BookmarkFilter { WorkflowInstanceId = resumeWorkflow.workflowExecutionId, Name = resumeWorkflow.Task }, cancellationToken);

        var bookmark = bookmarks.FirstOrDefault();

        if (bookmark is null)
        {
            var all = await _bookmarkStore.FindManyAsync(new BookmarkFilter { WorkflowInstanceId = resumeWorkflow.workflowExecutionId }, cancellationToken);

            bookmark = all.FirstOrDefault(bm => MatchesEventName(bm.Payload, resumeWorkflow.Task));
        }

        if (bookmark is null)
        {
            // Provide a diagnostic list of what events are waiting so the caller knows what names to use.
            var all = await _bookmarkStore.FindManyAsync(new BookmarkFilter { WorkflowInstanceId = resumeWorkflow.workflowExecutionId }, cancellationToken);

            var available = all.Select(bm => ExtractEventName(bm.Payload) ?? bm.Name ?? "(unnamed)").Distinct().ToList();

            var hint = available.Count > 0
                        ? $" Active event bookmarks for this instance: [{string.Join(", ", available)}]."
                        : " No active bookmarks found — the workflow may not have reached an Event activity yet.";

            throw new KeyNotFoundException($"No '{resumeWorkflow.workflowExecutionId}' event bookmark found for workflow ExecutionId '{resumeWorkflow.workflowExecutionId}'.{hint}");
        }
        try
        {
            var resumeResult = await _workflowResumer.ResumeAsync(bookmark.Id, new Dictionary<string, object> { ["action"] = resumeWorkflow.Action, ["payload"]= resumeWorkflow.Payload }, cancellationToken);
            _logger.LogInformation(
                "Workflow resumed successfully. ExecutionId: {ExecutionId}",
                resumeWorkflow.workflowExecutionId);
            var mappedBookmarks = resumeResult.Bookmarks?
                .Select(bookmark => new BookmarkResponseDto
                {
                    BookmarkId = bookmark.Id,
                    ActionName = ExtractEventName(bookmark.Payload),
                    ActionId = bookmark.ActivityId
                }).ToList() ?? new List<BookmarkResponseDto>();
            return new ResumeWorkflowResponse
            {
                WorkflowExecutionId = resumeResult.WorkflowInstanceId,
                Task = resumeWorkflow.Task,
                Action = resumeWorkflow.Action,
                Status = resumeResult.Status.ToString(),
                SubStatus = resumeResult.SubStatus.ToString(),
                Bookmark = mappedBookmarks
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                "Unexcepted error while resume workflow. ExecutionId: {ExecutionId}",
                resumeWorkflow.workflowExecutionId);
            throw ex;
        }
    }
    public async Task<WorkflowDefinitionPagedResponse> GetWorkflowDefinitionsAsync(
    WorkflowDefinitionQueryRequest request,
     ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GetWorkflowDefinitions started — Filters: {Filters}, SortBy: {SortBy}",
            request.Filters != null
                ? string.Join(", ", request.Filters.Select(f => $"{f.Key}={f.Value}"))
                : "none",
            request.SortBy ?? "none");

        // ── Step 1: Get all workflow definitions ───────────────────
        var filter = new WorkflowDefinitionFilter();

        var allDefinitions = await _definitionStore
            .FindManyAsync(filter, cancellationToken);

        var query = allDefinitions.AsQueryable();

        // ── Step 2: Apply filters if provided ─────────────────────
        if (request.Filters != null && request.Filters.Any())
        {
            foreach (var filterItem in request.Filters)
            {
                var column = filterItem.Key.ToLower().Trim();
                var value = filterItem.Value.Trim();

                _logger.LogInformation(
                    "Applying filter — Column: {Column}, Value: {Value}",
                    column, value);

                query = column switch
                {
                    "name" => query.Where(d =>
                        d.Name.Contains(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "definitionid" => query.Where(d =>
                        d.DefinitionId.Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "ispublished" => bool.TryParse(value, out var isPublished)
                        ? query.Where(d => d.IsPublished == isPublished)
                        : query,

                    "islatest" => bool.TryParse(value, out var isLatest)
                        ? query.Where(d => d.IsLatest == isLatest)
                        : query,

                    "version" => int.TryParse(value, out var version)
                        ? query.Where(d => d.Version == version)
                        : query,

                    "description" => query.Where(d =>
                        d.Description != null &&
                        d.Description.Contains(value,
                            StringComparison.OrdinalIgnoreCase)),

                    _ => query  // ← unknown column — ignore
                };
            }
        }

        // ── Step 3: Apply sorting ──────────────────────────────────
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var sortDirection = request.SortDirection ?? "ASC";

            // ── Apply sorting ──────────────────────────────────────────
            var isDesc = sortDirection
                .Equals("DESC", StringComparison.OrdinalIgnoreCase);

            query = request.SortBy.ToLower() switch
            {
                "name" => isDesc
                    ? query.OrderByDescending(d => d.Name)
                    : query.OrderBy(d => d.Name),

                "version" => isDesc
                    ? query.OrderByDescending(d => d.Version)
                    : query.OrderBy(d => d.Version),

                "createdat" => isDesc
                    ? query.OrderByDescending(d => d.CreatedAt)
                    : query.OrderBy(d => d.CreatedAt),

                "ispublished" => isDesc
                    ? query.OrderByDescending(d => d.IsPublished)
                    : query.OrderBy(d => d.IsPublished),

                _ => query.OrderBy(d => d.Name)  // ← default sort
            };
        }

        // ── Step 4: Get total count before pagination ──────────────
        var totalCount = query.Count();

        // ── Step 5: Apply pagination ───────────────────────────────
        List<WorkflowDefinition> pagedItems;
        // ── Pagination ─────────────────────────────────────────────
        if (request.PageNumber > 0 && request.PageSize > 0)
        {
            pagedItems = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
        }
        else
        {
            // Return all records when pagination is not specified
            pagedItems = query.ToList();

            request.PageNumber = 1;
            request.PageSize = pagedItems.Count;
        }
        var totalPages = request.PageSize > 0
            ? (int)Math.Ceiling((double)totalCount / request.PageSize)
            : 1;

        _logger.LogInformation(
            "GetWorkflowDefinitions completed — TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}",
            totalCount,
            request.PageNumber,
            request.PageSize);

        // ── Step 6: Map to response ────────────────────────────────
        var mappedItems = pagedItems.Select(definition => new WorkflowDefinitionResponse
        {
            DefinitionId = definition.DefinitionId,
            Name = definition.Name,
            Version = definition.Version,
            IsPublished = definition.IsPublished,
            IsLatest = definition.IsLatest,
            Description = definition.Description,
            CreatedAt = definition.CreatedAt
        }).ToList();

        return new WorkflowDefinitionPagedResponse
        {
            Items = mappedItems,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }


    public async Task<ActivityExecutionBriefPagedResponse> GetActivityExecutionsAsync(
    WorkflowDefinitionQueryRequest request,
    string ExecutionId,
    ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GetActivityExecutions started — Filters: {Filters}, SortBy: {SortBy}",
            request.Filters != null
                ? string.Join(", ", request.Filters.Select(f => $"{f.Key}={f.Value}"))
                : "none",
            request.SortBy ?? "none");

        // Step 1: Get all activity execution records
        var filter = new ActivityExecutionRecordFilter
        {
            WorkflowInstanceId = ExecutionId
        };

        var allRecords = await _activityExecutionStore
            .FindManyAsync(filter, cancellationToken);

        var query = allRecords.AsQueryable();

        // Step 2: Apply filters
        if (request.Filters != null && request.Filters.Any())
        {
            foreach (var filterItem in request.Filters)
            {
                var column = filterItem.Key.ToLower().Trim();
                var value = filterItem.Value.Trim();

                _logger.LogInformation(
                    "Applying filter — Column: {Column}, Value: {Value}",
                    column, value);

                query = column switch
                {
                    "workflowinstanceid" => query.Where(x =>
                        x.WorkflowInstanceId.Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "activityid" => query.Where(x =>
                        x.ActivityId.Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "activitytype" => query.Where(x =>
                        x.ActivityType.Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "status" => query.Where(x =>
                        x.Status.ToString().Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    _ => query
                };
            }
        }

        // Step 3: Apply sorting
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var sortDirection = request.SortDirection ?? "ASC";

            var isDesc = sortDirection.Equals(
                "DESC",
                StringComparison.OrdinalIgnoreCase);

            query = string.IsNullOrWhiteSpace(request.SortBy)
        ? query.OrderBy(x => x.StartedAt)
        : request.SortBy.ToLower() switch
        {
            "startedat" => isDesc
                ? query.OrderByDescending(x => x.StartedAt)
                : query.OrderBy(x => x.StartedAt),

            "completedat" => isDesc
                ? query.OrderByDescending(x => x.CompletedAt)
                : query.OrderBy(x => x.CompletedAt),

            "activityid" => isDesc
                ? query.OrderByDescending(x => x.ActivityId)
                : query.OrderBy(x => x.ActivityId),

            "activitytype" => isDesc
                ? query.OrderByDescending(x => x.ActivityType)
                : query.OrderBy(x => x.ActivityType),

            "status" => isDesc
                ? query.OrderByDescending(x => x.Status)
                : query.OrderBy(x => x.Status),

            _ => query.OrderBy(x => x.StartedAt)
        };
        }

        // Step 4: Total count
        var totalCount = query.Count();

        // Step 5: Pagination
        List<ActivityExecutionRecord> pagedItems;

        if (request.PageNumber > 0 && request.PageSize > 0)
        {
            pagedItems = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
        }
        else
        {
            pagedItems = query.ToList();

            request.PageNumber = 1;
            request.PageSize = pagedItems.Count;
        }

        var totalPages = request.PageSize > 0
            ? (int)Math.Ceiling((double)totalCount / request.PageSize)
            : 1;

        _logger.LogInformation(
            "GetActivityExecutions completed — TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}",
            totalCount,
            request.PageNumber,
            request.PageSize);

        // Step 6: Map response
        var mappedItems = pagedItems.Select(record => new ActivityExecutionBriefResponse
        {
            ActivityId = record.ActivityId,
            ActivityType = record.ActivityType,
            Status = record.Status.ToString(),
            StartedAt = record.StartedAt,
            CompletedAt = record.CompletedAt,
            Output = record.Payload
        }).ToList();

        return new ActivityExecutionBriefPagedResponse
        {
            Items = mappedItems,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
        };
    }



    public async Task<ActivityExecutionDetailPagedResponse> GetActivityExecutionsFullAsync(
    WorkflowDefinitionQueryRequest request,
    string executionId,
    ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GetActivityExecutionsDetail started — ExecutionId: {ExecutionId}",
            executionId);

        // ── DB level filter ────────────────────────────────────────
        var filter = new ActivityExecutionRecordFilter
        {
            WorkflowInstanceId = executionId
        };

        var allRecords = await _activityExecutionStore
            .FindManyAsync(filter, cancellationToken);

        var query = allRecords.AsQueryable();

        // ── In-memory filters ──────────────────────────────────────
        if (request.Filters != null && request.Filters.Any())
        {
            foreach (var filterItem in request.Filters)
            {
                var column = filterItem.Key.ToLower().Trim();
                var value = filterItem.Value.Trim();

                query = column switch
                {
                    "activityid" => query.Where(x =>
                        x.ActivityId.Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "activitytype" => query.Where(x =>
                        x.ActivityType.Contains(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "activityname" => query.Where(x =>
                        x.ActivityName != null &&
                        x.ActivityName.Contains(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "status" => query.Where(x =>
                        x.Status.ToString().Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "hasbookmarks" => bool.TryParse(value, out var hasBookmarks)
                        ? query.Where(x => x.HasBookmarks == hasBookmarks)
                        : query,


                    "startedfrom" => DateTimeOffset.TryParse(value, out var startedFrom)
                        ? query.Where(x => x.StartedAt >= startedFrom)
                        : query,


                    _ => query
                };
            }
        }

        // ── Sorting ────────────────────────────────────────────────
        var isDesc = (request.SortDirection ?? "ASC")
            .Equals("DESC", StringComparison.OrdinalIgnoreCase);

        query = string.IsNullOrWhiteSpace(request.SortBy)
            ? query.OrderBy(x => x.StartedAt)
            : request.SortBy.ToLower() switch
            {
                "startedat" => isDesc
                    ? query.OrderByDescending(x => x.StartedAt)
                    : query.OrderBy(x => x.StartedAt),
                "completedat" => isDesc
                    ? query.OrderByDescending(x => x.CompletedAt)
                    : query.OrderBy(x => x.CompletedAt),
                "activityid" => isDesc
                    ? query.OrderByDescending(x => x.ActivityId)
                    : query.OrderBy(x => x.ActivityId),
                "activitytype" => isDesc
                    ? query.OrderByDescending(x => x.ActivityType)
                    : query.OrderBy(x => x.ActivityType),
                "status" => isDesc
                    ? query.OrderByDescending(x => x.Status)
                    : query.OrderBy(x => x.Status),
                _ => query.OrderBy(x => x.StartedAt)
            };

        // ── Total count ────────────────────────────────────────────
        var totalCount = query.Count();

        // ── Pagination ─────────────────────────────────────────────
        List<ActivityExecutionRecord> pagedItems;

        if (request.PageNumber > 0 && request.PageSize > 0)
        {
            pagedItems = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
        }
        else
        {
            pagedItems = query.ToList();
            request.PageNumber = 1;
            request.PageSize = pagedItems.Count;
        }

        _logger.LogInformation(
            "GetActivityExecutionsFull completed — TotalCount: {TotalCount}",
            totalCount);

        // ── Map everything ─────────────────────────────────────────
        var mappedItems = pagedItems.Select(record => new ActivityExecutionDetailResponse
        {
            Id = record.Id,
            WorkflowInstanceId = record.WorkflowInstanceId,
            ActivityId = record.ActivityId,
            ActivityNodeId = record.ActivityNodeId,
            ActivityType = record.ActivityType,
            ActivityTypeVersion = record.ActivityTypeVersion,
            ActivityName = record.ActivityName,
            Status = record.Status.ToString(),
            HasBookmarks = record.HasBookmarks,
            Payload = record.Payload,
            Outputs = record.Outputs,
            Properties = record.Properties,
            Exception = record.Exception?.Message,
            StartedAt = record.StartedAt,
            CompletedAt = record.CompletedAt
        }).ToList();

        return new ActivityExecutionDetailPagedResponse
        {
            Items = mappedItems,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
    // Helper to check if the bookmark's payload contains an eventName that matches the given event name.
    private static bool MatchesEventName(object? payload, string eventName)
    {
        var extracted = ExtractEventName(payload);
        return extracted is not null && string.Equals(extracted, eventName, StringComparison.OrdinalIgnoreCase);
    }

    // Helper to extract an "eventName" property from the bookmark's payload, if it exists. This allows matching bookmarks based on the event name even if the bookmark's Name is not set to the event name.
    private static string? ExtractEventName(object? payload)
    {
        if (payload is null)
        {
            return null;
        }

        try
        {
            var json = JsonSerializer.Serialize(payload);
            var doc = JsonDocument.Parse(json);

            var prop = doc.RootElement.EnumerateObject()
                .FirstOrDefault(p => string.Equals(p.Name, "eventName", StringComparison.OrdinalIgnoreCase));

            return prop.Value.ValueKind == JsonValueKind.String ? prop.Value.GetString() : null;
        }
        catch
        {
            return null;
        }
    }
    public async Task<WorkflowDefinitionDetailPagedResponse> GetWorkflowDefinitionsDetailAsync(
    WorkflowDefinitionQueryRequest request,
    ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GetWorkflowDefinitionsDetail started — Filters: {Filters}",
            request.Filters != null
                ? string.Join(", ", request.Filters.Select(f => $"{f.Key}={f.Value}"))
                : "none");

        // ── Get all definitions ────────────────────────────────────
        var allDefinitions = await _definitionStore
            .FindManyAsync(new WorkflowDefinitionFilter(), cancellationToken);

        var query = allDefinitions.AsQueryable();

        // ── Apply filters ──────────────────────────────────────────
        if (request.Filters != null && request.Filters.Any())
        {
            foreach (var filterItem in request.Filters)
            {
                var column = filterItem.Key.ToLower().Trim();
                var value = filterItem.Value.Trim();

                query = column switch
                {
                    "name" => query.Where(d =>
                        d.Name.Contains(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "definitionid" => query.Where(d =>
                        d.DefinitionId.Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "ispublished" => bool.TryParse(value, out var isPublished)
                        ? query.Where(d => d.IsPublished == isPublished)
                        : query,

                    "islatest" => bool.TryParse(value, out var isLatest)
                        ? query.Where(d => d.IsLatest == isLatest)
                        : query,

                    "version" => int.TryParse(value, out var version)
                        ? query.Where(d => d.Version == version)
                        : query,

                    "description" => query.Where(d =>
                        d.Description != null &&
                        d.Description.Contains(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "tenantid" => query.Where(d =>
                        d.TenantId != null &&
                        d.TenantId.Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "createdfrom" => DateTimeOffset.TryParse(value, out var createdFrom)
                        ? query.Where(d => d.CreatedAt >= createdFrom)
                        : query,

                    "createdto" => DateTimeOffset.TryParse(value, out var createdTo)
                        ? query.Where(d => d.CreatedAt <= createdTo)
                        : query,

                    _ => query
                };
            }
        }
        var sortDirection = request.SortDirection ?? "ASC";

        // ── Apply sorting ──────────────────────────────────────────
        var isDesc = sortDirection
            .Equals("DESC", StringComparison.OrdinalIgnoreCase);

        query = string.IsNullOrWhiteSpace(request.SortBy)
            ? query.OrderBy(d => d.Name)
            : request.SortBy.ToLower() switch
            {
                "name" => isDesc
                    ? query.OrderByDescending(d => d.Name)
                    : query.OrderBy(d => d.Name),
                "version" => isDesc
                    ? query.OrderByDescending(d => d.Version)
                    : query.OrderBy(d => d.Version),
                "createdat" => isDesc
                    ? query.OrderByDescending(d => d.CreatedAt)
                    : query.OrderBy(d => d.CreatedAt),
                "ispublished" => isDesc
                ? query.OrderByDescending(d => d.IsPublished)
                : query.OrderBy(d => d.IsPublished),
                _ => query.OrderBy(d => d.Name)
            };

        // ── Total count ────────────────────────────────────────────
        var totalCount = query.Count();
        List<WorkflowDefinition> pagedItems;
        // ── Pagination ─────────────────────────────────────────────
        if (request.PageNumber > 0 && request.PageSize > 0)
        {
            pagedItems = query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();
        }
        else
        {
            // Return all records when pagination is not specified
            pagedItems = query.ToList();

            request.PageNumber = 1;
            request.PageSize = pagedItems.Count;
        }
        var totalPages = request.PageSize > 0
            ? (int)Math.Ceiling((double)totalCount / request.PageSize)
            : 1;

        _logger.LogInformation(
            "GetWorkflowDefinitionsDetail completed — TotalCount: {TotalCount}",
            totalCount);

        // ── Map to detail response ─────────────────────────────────
        var mappedItems = pagedItems.Select(d => new WorkflowDefinitionDetailResponse
        {
            // ── Existing fields ────────────────────────────────────
            DefinitionId = d.DefinitionId,
            Name = d.Name,
            Version = d.Version,
            IsPublished = d.IsPublished,
            IsLatest = d.IsLatest,
            IsReadonly = d.IsReadonly,
            IsSystem = d.IsSystem,
            Description = d.Description,
            TenantId = d.TenantId,
            CreatedAt = d.CreatedAt,

            // ── New parsed fields ──────────────────────────────────
            StringData = ParseStringData(d.StringData),
            //Props = ParseProps(d.Props)
        }).ToList();

        return new WorkflowDefinitionDetailPagedResponse
        {
            Items = mappedItems,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static JsonElement? ParseStringData(string? stringData)
    {
        if (string.IsNullOrWhiteSpace(stringData))
            return null;

        try
        {
            // Handle double-serialized JSON if necessary.
            if (stringData.StartsWith("\"{"))
            {
                stringData = JsonSerializer.Deserialize<string>(stringData);
            }

            return JsonSerializer.Deserialize<JsonElement>(
                stringData!,
                JsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse StringData: {ex.Message}");
            return null;
        }
    }
    public async Task<LastExecutedWorkflowActivityResponse> GetLastExecutedWorkflowActivityAsync(
    WorkflowDefinitionQueryRequest request,
    string DefinitionId,
    ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GetLastExecutedWorkflowActivity started — DefinitionId: {DefinitionId}",
            DefinitionId);

        // ── Step 1: Validate ───────────────────────────────────────
        if (string.IsNullOrWhiteSpace(DefinitionId))
            throw new ArgumentException(
                "DefinitionId is required",
                nameof(DefinitionId));

        // ── Step 2: Get all instances by DefinitionId ──────────────
        var instanceFilter = new WorkflowInstanceFilter
        {
            DefinitionId = DefinitionId
        };

        var allInstances = await _instanceStore
            .FindManyAsync(instanceFilter, cancellationToken);

        var instanceList = allInstances.ToList();

        _logger.LogInformation(
            "Found {Count} workflow instances for DefinitionId: {DefinitionId}",
            instanceList.Count,
            DefinitionId);

        // ── Step 3: No instances found ─────────────────────────────
        if (!instanceList.Any())
            throw new KeyNotFoundException(
                $"No workflow instances found for DefinitionId '{DefinitionId}'");

        // ── Step 4: Get last executed instance ─────────────────────
        var lastInstance = instanceList
            .OrderByDescending(i => i.CreatedAt)
            .First();

        _logger.LogInformation(
            "Last executed instance — InstanceId: {InstanceId}, Status: {Status}, CreatedAt: {CreatedAt}",
            lastInstance.Id,
            lastInstance.Status,
            lastInstance.CreatedAt);

        // ── Step 5: Get activities for last instance ───────────────
        var activityFilter = new ActivityExecutionRecordFilter
        {
            WorkflowInstanceId = lastInstance.Id
        };

        var allRecords = await _activityExecutionStore
            .FindManyAsync(activityFilter, cancellationToken);

        var query = allRecords.AsQueryable();

        // ── Step 6: Apply activity filters if provided ─────────────
        if (request.Filters != null &&
            request.Filters.Any())
        {
            foreach (var filterItem in request.Filters)
            {
                var column = filterItem.Key.ToLower().Trim();
                var value = filterItem.Value.Trim();

                query = column switch
                {
                    "activityid" => query.Where(x =>
                        x.ActivityId.Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "activitytype" => query.Where(x =>
                        x.ActivityType.Contains(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "activityname" => query.Where(x =>
                        x.ActivityName != null &&
                        x.ActivityName.Contains(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "status" => query.Where(x =>
                        x.Status.ToString().Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "hasbookmarks" => bool.TryParse(value, out var hasBookmarks)
                        ? query.Where(x => x.HasBookmarks == hasBookmarks)
                        : query,


                    _ => query
                };
            }
        }

        // ── Step 7: Apply sorting ──────────────────────────────────
        var sortBy = request.SortBy;
        var sortDirection = request.SortDirection ?? "ASC";
        var isDesc = sortDirection.Equals("DESC",
            StringComparison.OrdinalIgnoreCase);

        query = string.IsNullOrWhiteSpace(sortBy)
            ? query.OrderBy(x => x.StartedAt)
            : sortBy.ToLower() switch
            {
                "startedat" => isDesc
                    ? query.OrderByDescending(x => x.StartedAt)
                    : query.OrderBy(x => x.StartedAt),
                "completedat" => isDesc
                    ? query.OrderByDescending(x => x.CompletedAt)
                    : query.OrderBy(x => x.CompletedAt),
                "activityid" => isDesc
                    ? query.OrderByDescending(x => x.ActivityId)
                    : query.OrderBy(x => x.ActivityId),
                "activitytype" => isDesc
                    ? query.OrderByDescending(x => x.ActivityType)
                    : query.OrderBy(x => x.ActivityType),
                "status" => isDesc
                    ? query.OrderByDescending(x => x.Status)
                    : query.OrderBy(x => x.Status),
                _ => query.OrderBy(x => x.StartedAt)
            };

        // ── Step 8: Pagination ─────────────────────────────────────
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;
        var totalCount = query.Count();

        List<ActivityExecutionRecord> pagedItems;

        if (pageNumber > 0 && pageSize > 0)
        {
            pagedItems = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        else
        {
            pagedItems = query.ToList();

            pageNumber = 1;
            pageSize = pagedItems.Count;
        }

        _logger.LogInformation(
            "GetLastExecutedWorkflowActivity completed — " +
            "InstanceId: {InstanceId}, TotalActivities: {TotalCount}",
            lastInstance.Id,
            totalCount);

        // ── Step 9: Map activities ─────────────────────────────────
        var mappedActivities = pagedItems.Select(record =>
            new ActivityExecutionBriefResponse
            {
                ActivityId = record.ActivityId,
                ActivityType = record.ActivityType,
                Status = record.Status.ToString(),
                StartedAt = record.StartedAt,
                CompletedAt = record.CompletedAt,
                Output = record.Outputs
            }).ToList();

        // ── Step 10: Return ────────────────────────────────────────
        return new LastExecutedWorkflowActivityResponse
        {
            DefinitionId = DefinitionId,
            WorkflowInstanceId = lastInstance.Id,
            CreatedAt = lastInstance.CreatedAt,
            Activities = new ActivityExecutionBriefPagedResponse
            {
                Items = mappedActivities,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            }
        };
    }


    public async Task<LastExecutedWorkflowActivityDetailResponse> GetLastExecutedWorkflowActivityDetailAsync(
    WorkflowDefinitionQueryRequest request,
    string DefinitionId,
    ExecutionModel ctx,
    CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GetLastExecutedWorkflowDetailActivity started — DefinitionId: {DefinitionId}",
            DefinitionId);

        // ── Step 1: Validate ───────────────────────────────────────
        if (string.IsNullOrWhiteSpace(DefinitionId))
            throw new ArgumentException(
                "DefinitionId is required",
                nameof(DefinitionId));

        // ── Step 2: Get all instances by DefinitionId ──────────────
        var instanceFilter = new WorkflowInstanceFilter
        {
            DefinitionId = DefinitionId
        };

        var allInstances = await _instanceStore
            .FindManyAsync(instanceFilter, cancellationToken);

        var instanceList = allInstances.ToList();

        _logger.LogInformation(
            "Found {Count} workflow instances for DefinitionId: {DefinitionId}",
            instanceList.Count,
            DefinitionId);

        // ── Step 3: No instances found ─────────────────────────────
        if (!instanceList.Any())
            throw new KeyNotFoundException(
                $"No workflow instances found for DefinitionId '{DefinitionId}'");

        // ── Step 4: Get last executed instance ─────────────────────
        var lastInstance = instanceList
            .OrderByDescending(i => i.CreatedAt)
            .First();

        _logger.LogInformation(
            "Last executed instance — InstanceId: {InstanceId}, Status: {Status}, CreatedAt: {CreatedAt}",
            lastInstance.Id,
            lastInstance.Status,
            lastInstance.CreatedAt);

        // ── Step 5: Get activities for last instance ───────────────
        var activityFilter = new ActivityExecutionRecordFilter
        {
            WorkflowInstanceId = lastInstance.Id
        };

        var allRecords = await _activityExecutionStore
            .FindManyAsync(activityFilter, cancellationToken);

        var query = allRecords.AsQueryable();

        // ── Step 6: Apply activity filters if provided ─────────────
        if (request.Filters != null &&
            request.Filters.Any())
        {
            foreach (var filterItem in request.Filters)
            {
                var column = filterItem.Key.ToLower().Trim();
                var value = filterItem.Value.Trim();

                query = column switch
                {
                    "activityid" => query.Where(x =>
                        x.ActivityId.Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "activitytype" => query.Where(x =>
                        x.ActivityType.Contains(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "activityname" => query.Where(x =>
                        x.ActivityName != null &&
                        x.ActivityName.Contains(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "status" => query.Where(x =>
                        x.Status.ToString().Equals(value,
                            StringComparison.OrdinalIgnoreCase)),

                    "hasbookmarks" => bool.TryParse(value, out var hasBookmarks)
                        ? query.Where(x => x.HasBookmarks == hasBookmarks)
                        : query,


                    _ => query
                };
            }
        }

        // ── Step 7: Apply sorting ──────────────────────────────────
        var sortBy = request.SortBy;
        var sortDirection = request.SortDirection ?? "ASC";
        var isDesc = sortDirection.Equals("DESC",
            StringComparison.OrdinalIgnoreCase);

        query = string.IsNullOrWhiteSpace(sortBy)
            ? query.OrderBy(x => x.StartedAt)
            : sortBy.ToLower() switch
            {
                "startedat" => isDesc
                    ? query.OrderByDescending(x => x.StartedAt)
                    : query.OrderBy(x => x.StartedAt),
                "completedat" => isDesc
                    ? query.OrderByDescending(x => x.CompletedAt)
                    : query.OrderBy(x => x.CompletedAt),
                "activityid" => isDesc
                    ? query.OrderByDescending(x => x.ActivityId)
                    : query.OrderBy(x => x.ActivityId),
                "activitytype" => isDesc
                    ? query.OrderByDescending(x => x.ActivityType)
                    : query.OrderBy(x => x.ActivityType),
                "status" => isDesc
                    ? query.OrderByDescending(x => x.Status)
                    : query.OrderBy(x => x.Status),
                _ => query.OrderBy(x => x.StartedAt)
            };

        // ── Step 8: Pagination ─────────────────────────────────────
        var pageNumber = request.PageNumber;
        var pageSize = request.PageSize;
        var totalCount = query.Count();

        List<ActivityExecutionRecord> pagedItems;

        if (pageNumber > 0 && pageSize > 0)
        {
            pagedItems = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        else
        {
            pagedItems = query.ToList();

            pageNumber = 1;
            pageSize = pagedItems.Count;
        }

        _logger.LogInformation(
            "GetLastExecutedWorkflowActivity completed — " +
            "InstanceId: {InstanceId}, TotalActivities: {TotalCount}",
            lastInstance.Id,
            totalCount);

        // ── Step 9: Map activities ─────────────────────────────────
        var mappedItems = pagedItems.Select(record => new ActivityExecutionDetailResponse
        {
            Id = record.Id,
            WorkflowInstanceId = record.WorkflowInstanceId,
            ActivityId = record.ActivityId,
            ActivityNodeId = record.ActivityNodeId,
            ActivityType = record.ActivityType,
            ActivityTypeVersion = record.ActivityTypeVersion,
            ActivityName = record.ActivityName,
            Status = record.Status.ToString(),
            HasBookmarks = record.HasBookmarks,
            Payload = record.Payload,
            Outputs = record.Outputs,
            Properties = record.Properties,
            Exception = record.Exception?.Message,
            StartedAt = record.StartedAt,
            CompletedAt = record.CompletedAt
        }).ToList();

        // ── Step 10: Return ────────────────────────────────────────
        return new LastExecutedWorkflowActivityDetailResponse
        {
            DefinitionId = DefinitionId,
            WorkflowInstanceId = lastInstance.Id,
            CreatedAt = lastInstance.CreatedAt,
            Activities = new ActivityExecutionDetailPagedResponse
            {
                Items = mappedItems,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            }
        };
    }
    private static WorkflowPropsDto? ParseProps(string? props)
    {
        if (string.IsNullOrWhiteSpace(props))
            return null;
        try
        {
            return JsonSerializer.Deserialize<WorkflowPropsDto>(
                props, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    //public async Task ResumeWorkflowAsync(
    //    string executionId,
    //    string signal,
    //    object? payload,
    //    ExecutionModel ctx,
    //    CancellationToken cancellationToken = default)
    //{
    //    _logger.LogInformation("Resuming workflow {ExecutionId} with signal {Signal}",
    //        executionId, signal);

    //    var resumeInput = new Dictionary<string, object>
    //    {
    //        ["ResumePayload"] = payload ?? new { },
    //        ["ResumedBy"] = ctx.UserId,
    //        ["ResumedAt"] = DateTimeOffset.UtcNow
    //    };

    //    // Use bookmark ID as signal to resume
    //    await _workflowRuntime.ResumeWorkflowAsync(
    //        executionId,
    //        signal,  // BookmarkId
    //        resumeInput,
    //        cancellationToken);

    //    _logger.LogInformation("Workflow {ExecutionId} resumed successfully", executionId);
    //}

    //public async Task CancelWorkflowAsync(
    //    string executionId,
    //    string reason,
    //    ExecutionModel ctx,
    //    CancellationToken cancellationToken = default)
    //{
    //    _logger.LogInformation("Cancelling workflow {ExecutionId}. Reason: {Reason}",
    //        executionId, reason);

    //    var instance = await _instanceStore.FindByIdAsync(executionId, cancellationToken);
    //    if (instance == null)
    //    {
    //        throw new WorkflowNotFoundException($"Workflow execution {executionId} not found");
    //    }

    //    await _workflowRuntime.CancelWorkflowAsync(executionId, cancellationToken);

    //    _logger.LogInformation("Workflow {ExecutionId} cancelled", executionId);
    //}

    public async Task<WorkflowExecutionState> GetExecutionStateAsync(
        string executionId,
        CancellationToken cancellationToken = default)
    {
        var filter = new WorkflowInstanceFilter
        {
            DefinitionId = executionId
        };
        var instance = await _instanceStore.FindAsync(filter, cancellationToken);

        if (instance == null)
        {
            throw new WorkflowNotFoundException($"Workflow execution {executionId} not found");
        }

        var state = new WorkflowExecutionState
        {
            ExecutionId = instance.Id,
            Status = MapElsaStatusToExecutionStatus(instance.Status),
            SubStatus = MapElsaSubStatusToExecutionStatus(instance.SubStatus),
            StartedAt = instance.CreatedAt,
            CompletedAt = instance.FinishedAt,
            Variables = new Dictionary<string, object>()
        };

        // Parse workflow state from SerializedWorkflowState if available
        //if (!string.IsNullOrWhiteSpace(instance.SerializedWorkflowState))
        //{
        //    try
        //    {
        //        var workflowState = JsonSerializer.Deserialize<Dictionary<string, object>>(
        //            instance.SerializedWorkflowState);
        //        state.Variables = workflowState ?? new Dictionary<string, object>();
        //    }
        //    catch (JsonException ex)
        //    {
        //        _logger.LogWarning(ex, "Failed to deserialize workflow state for {ExecutionId}", executionId);
        //    }
        //}

        return state;
    }

    //public async Task<PagedResult<WorkflowExecutionSummary>> ListExecutionsAsync(
    //    string definitionId,
    //    WorkflowStatusFilter? status = null,
    //    int page = 1,
    //    int pageSize = 20,
    //    CancellationToken cancellationToken = default)
    //{
    //    // Build filter
    //    var filter = new WorkflowInstanceFilter
    //    {
    //        DefinitionId = definitionId
    //    };

    //    if (status.HasValue && status != WorkflowStatusFilter.All)
    //    {
    //        filter.Status = MapStatusFilterToElsaString(status.Value);
    //    }

    //    // Use Elsa's pagination
    //    var pageArgs = PageArgs.FromPage(page, pageSize);
    //    var pageResult = await _instanceStore.FindManyAsync(filter, pageArgs, cancellationToken);

    //    var items = pageResult.Items.Select(instance => new WorkflowExecutionSummary
    //    {
    //        ExecutionId = instance.Id,
    //        EngineExecutionId = instance.Id,
    //        Status = MapElsaStatusToExecutionStatus(instance.Status),
    //        StartedAt = instance.CreatedAt,
    //        CompletedAt = instance.FinishedAt,
    //        CorrelationId = instance.CorrelationId
    //    }).ToList();

    //    return new PagedResult<WorkflowExecutionSummary>
    //    {
    //        Items = items,
    //        TotalCount = (int)pageResult.TotalCount,
    //        Page = page,
    //        PageSize = pageSize
    //    };
    //}
    private async Task<WorkflowResumeResult> ResumeHandleAsync(
    string WorkflowExecutionId,
    string signal,
    string decision,
    CancellationToken ct)
    {
        try
        {
            _logger.LogInformation(
                "Processing workflow decision. WorkflowExecutionId: {WorkflowExecutionId}, Decision: {Decision}",
                WorkflowExecutionId,
                decision);

            if (string.IsNullOrWhiteSpace(WorkflowExecutionId))
            {
                _logger.LogWarning("Workflow decision failed because WorkflowExecutionId is null or empty.");

                throw new ArgumentException(
                    "WorkflowExecutionId is required",
                    nameof(WorkflowExecutionId));
            }

            var bookmarks = await _bookmarkStore.FindManyAsync(
                new BookmarkFilter
                {
                    WorkflowInstanceId = WorkflowExecutionId,
                    Name = decision
                },
                ct);

            var bookmark = bookmarks.FirstOrDefault()
                ?? (await _bookmarkStore.FindManyAsync(
                    new BookmarkFilter
                    {
                        WorkflowInstanceId = WorkflowExecutionId
                    },
                    ct)).FirstOrDefault();

            if (bookmark is null)
            {
                _logger.LogWarning(
                    "No bookmark found for Workflow ExecutionId {ExecutionId}",
                    WorkflowExecutionId);

                throw new KeyNotFoundException(
                    $"No bookmark found for workflow ExecutionId '{WorkflowExecutionId}'.");
            }

            _logger.LogInformation(
                "Resuming workflow. BookmarkId: {BookmarkId}, Signal: {Signal}, ExecutionId: {ExecutionId}",
                bookmark.Id,
                signal,
                WorkflowExecutionId);

            var resumeResult = await _workflowResumer.ResumeAsync(
                bookmark.Id,
                new Dictionary<string, object>
                {
                    ["decision"] = decision,
                    ["signal"] = signal,
                    ["reviewedBy"] = "system"
                },
                ct);
            if (resumeResult == null)
                throw new Exception($"Resume returned null for bookmark {bookmark.Id}");

            // ── Map status ─────────────────────────────────────────────────
            var status = MapElsaStatusToExecutionStatus(resumeResult.Status);
            var subStatus = MapElsaSubStatusToExecutionStatus(resumeResult.SubStatus);

            _logger.LogInformation(
                "Workflow resumed — InstanceId: {InstanceId}, Status: {Status}, SubStatus: {SubStatus}",
                resumeResult.WorkflowInstanceId,
                status,
                subStatus);

            // ── Check if faulted ───────────────────────────────────────────
            if (resumeResult.Status.ToString() == "Faulted" ||
                resumeResult.SubStatus.ToString() == "Faulted")
            {
                var incidents = resumeResult.Incidents ?? new List<ActivityIncident>();

                foreach (var incident in incidents)
                {
                    _logger.LogError(
                        "Workflow incident — ActivityId: {ActivityId}, Message: {Message}",
                        incident.ActivityId,
                        incident.Message);
                }

                throw new WorkflowEngineException("Elsa",
                    $"Workflow faulted after resume. Incidents: {incidents.Count}");
            }

            // ── Return result ──────────────────────────────────────────────
            return new WorkflowResumeResult
            {
                ExecutionId = resumeResult.WorkflowInstanceId,
                Status = status.ToString(),
                SubStatus = subStatus.ToString()
            };
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (KeyNotFoundException)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(
                ex,
                "Workflow decision operation was cancelled. ExecutionId: {ExecutionId}",
                WorkflowExecutionId);

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while processing workflow decision. ExecutionId: {ExecutionId}",
                WorkflowExecutionId);

            throw;
        }
    }
    
    public async Task<ValidationResult> ValidateDefinitionAsync(
        Workflowdefinition definition,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(definition.DefinitionId))
            {
                return ValidationResult.Fail("DefinitionId is required");
            }

            if (definition.Steps == null || !definition.Steps.Any())
            {
                return ValidationResult.Fail("Workflow must have at least one step");
            }

            // Try to convert to Elsa JSON format (this validates the structure)
            var elsaJson = _mapper.ConvertToElsaJson(definition);

            // Validate JSON is parsable
            using var doc = JsonDocument.Parse(elsaJson);

            // Check for name uniqueness in Elsa
            var isNameUnique = await _definitionStore.GetIsNameUnique(
                definition.WorkflowName,
                definition.DefinitionId,
                cancellationToken);

            if (!isNameUnique)
            {
                return ValidationResult.Fail($"Workflow name '{definition.WorkflowName}' is not unique");
            }

            // Validate each action's configuration
            foreach (var step in definition.Steps)
            {
                foreach (var task in step.Tasks)
                {
                    foreach (var action in task.Actions)
                    {
                        var actionValidation = await ValidateActionConfigAsync(action);
                        if (!actionValidation.IsValid)
                        {
                            return actionValidation;
                        }
                    }
                }
            }

            return ValidationResult.Success();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON validation failed for workflow {DefinitionId}", definition.DefinitionId);
            return ValidationResult.Fail($"Invalid workflow JSON: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validation failed for workflow {DefinitionId}", definition.DefinitionId);
            return ValidationResult.Fail($"Validation error: {ex.Message}");
        }
    }

    //public async Task<EngineHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    //{
    //    try
    //    {
    //        // Try to count definitions to check connectivity
    //        var filter = new WorkflowDefinitionFilter();
    //        var pageArgs = PageArgs.FromPage(1, 1);
    //        var result = await _definitionStore.FindManyAsync(filter, pageArgs, cancellationToken);

    //        return new EngineHealthStatus
    //        {
    //            IsHealthy = true,
    //            EngineType = EngineType,
    //            Version = "3.1.3",
    //            LastHeartbeat = DateTimeOffset.UtcNow,
    //            Details = new Dictionary<string, string>
    //            {
    //                ["ElsaVersion"] = "3.1.3",
    //                ["DefinitionsCount"] = "Available via query"
    //            }
    //        };
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Elsa engine health check failed");

    //        return new EngineHealthStatus
    //        {
    //            IsHealthy = false,
    //            EngineType = EngineType,
    //            Version = "3.1.3",
    //            LastHeartbeat = DateTimeOffset.UtcNow,
    //            Warnings = { $"Health check failed: {ex.Message}" }
    //        };
    //    }
    //}

    //#region Private Helper Methods

    private WorkflowExecutionStatus MapElsaStatusToExecutionStatus(ElsaStatus status)
    {
        return status switch
        {
            ElsaStatus.Running => WorkflowExecutionStatus.Running,
            //ElsaStatus.Suspended => WorkflowExecutionStatus.Suspended,
            ElsaStatus.Finished => WorkflowExecutionStatus.Completed,
            //ElsaStatus.Faulted => WorkflowExecutionStatus.Failed,
            //ElsaStatus.Cancelled => WorkflowExecutionStatus.Cancelled,
            _ => WorkflowExecutionStatus.Running
        };
    }

    private WorkflowExecutionSubStatus MapElsaSubStatusToExecutionStatus(ElsaSubStatus subStatus)
    {
        return subStatus switch
        {
            //ElsaSubStatus.Running => WorkflowExecutionStatus.Running,
            ElsaSubStatus.Suspended => WorkflowExecutionSubStatus.Suspended,
            ElsaSubStatus.Finished => WorkflowExecutionSubStatus.Completed,
            ElsaSubStatus.Faulted => WorkflowExecutionSubStatus.Failed,
            ElsaSubStatus.Cancelled => WorkflowExecutionSubStatus.Cancelled,
            _ => WorkflowExecutionSubStatus.Running
        };
    }

    private async Task<ValidationResult> ValidateActionConfigAsync(WorkflowAction action)
    {
        return action.ActionType switch
        {
            "SendEmail" => ValidateEmailAction(action),
            "HttpCall" => ValidateHttpAction(action),
            "Wait" => ValidationResult.Success(),
            _ => ValidationResult.Success()
        };
    }

    private ValidationResult ValidateEmailAction(WorkflowAction action)
    {
        var config = action.GetConfig<EmailConfig>();

        if (string.IsNullOrWhiteSpace(config.To))
            return ValidationResult.Fail("Email action requires 'To' address");

        if (string.IsNullOrWhiteSpace(config.Subject))
            return ValidationResult.Fail("Email action requires 'Subject'");

        return ValidationResult.Success();
    }

    private ValidationResult ValidateHttpAction(WorkflowAction action)
    {
        var config = action.GetConfig<HttpConfig>();

        if (string.IsNullOrWhiteSpace(config.Url))
            return ValidationResult.Fail("HTTP action requires 'Url'");

        return ValidationResult.Success();
    }



    //#endregion
}

//// Example: SendEmail action execution
//public class SendEmailActionExecutor : IActionExecutor
//{
//    public async Task ExecuteAsync(WorkflowAction action, ActionContext ctx)
//    {
//        var config = action.GetConfig<EmailConfig>();

//        if (string.IsNullOrWhiteSpace(config.To))
//            throw new InvalidOperationException("Email 'To' address is required");

//        // Send email using your email service
//        await _emailService.SendAsync(
//            to: config.To,
//            subject: config.Subject,
//            body: config.Body,
//            isHtml: config.IsBodyHtml,
//            cc: config.Cc,
//            bcc: config.Bcc
//        );
//    }
//}


//public ElsaWorkflowAdapter(IWorkflowDefinitionStore definitionStore, IWorkflowRuntime workflowRuntime)
//{
//    _definitionStore = definitionStore;
//    _workflowRuntime = workflowRuntime;
//}

//public string EngineType => "Elsa";

//public async Task RegisterDefinitionAsync(Workflowdefinition definition, CancellationToken ct = default)
//{
//    // Convert our WorkflowDefinition to Elsa's WorkflowDefinition model
//    var elsaDefinition = MapToElsaDefinition(definition);
//    await _definitionStore.SaveAsync(elsaDefinition, ct);
//}

// Other methods (StartWorkflowAsync, ResumeWorkflowAsync) will be added later