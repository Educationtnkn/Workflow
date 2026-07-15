

using EnterpriseWorkflow.Domain.Configurations;
using EnterpriseWorkflow.Domain.Entities.Workflow;
using EnterpriseWorkflow.Domain.Enums;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EnterpriseWorkflow.Elsa.Mapping
{
    /// <summary>
    /// Converts an enterprise WorkflowDefinition into Elsa 3.x StringData JSON.
    /// Produces the same flat-flowchart JSON shape that Elsa's Json materializer expects.
    /// </summary>
    public sealed class EnterpriseToElsaMapper
    {
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // ════════════════════════════════════════════════════════════════════
        // Public entry point
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Returns the full Elsa StringData JSON string — assign directly to
        /// WorkflowDefinition.StringData before calling IWorkflowDefinitionStore.SaveAsync.
        /// </summary>
        //public string ConvertToElsaJson(Workflowdefinition definition)
        //{
        //    // 1. Build all activity nodes (same as before, but collect all actions)
        //    var activities = BuildAllActivities(definition);

        //    // 2. Build connections from WorkflowTransition list (now part of definition)
        //    var connections = BuildConnectionsFromTransitions(definition.Transitions);

        //    // 3. Create root flowchart
        //    var root = new JsonObject
        //    {
        //        ["id"] = $"{definition.DefinitionId}_Flowchart",
        //        ["type"] = "Elsa.Flowchart",
        //        ["activities"] = activities,
        //        ["connections"] = connections
        //    };

        //    // 4. Wrap and return (same envelope)
        //    return WrapInModel(definition, root);
        //}
        //   public List<WorkflowStep> BuildStepsFromTasksAndTransitions(
        //List<TaskDefinition> tasks,   // from your DB (TaskDefinition table)
        //List<WorkflowTransition> transitions)
        //   {
        //       var steps = new List<WorkflowStep>();

        //       // 1. Get the sequential order from transitions with TransitionMode = "Sequential" and PathType = "Success"
        //       var sequentialTransitions = transitions
        //           .Where(t => t.TransitionMode == "Sequential" && t.TransitionPathType == "Success")
        //           .OrderBy(t => t.Priority)
        //           .ToList();

        //       var orderedTaskIds = new List<int>();
        //       var first = sequentialTransitions.FirstOrDefault();
        //       if (first != null)
        //       {
        //           var currentId = first.FromNodeId;
        //           while (currentId != 0)
        //           {
        //               orderedTaskIds.Add(currentId);
        //               var next = sequentialTransitions.FirstOrDefault(t => t.FromNodeId == currentId);
        //               currentId = next?.ToNodeId ?? 0;
        //           }
        //       }

        //       int stepSeq = 1;
        //       foreach (var taskId in orderedTaskIds)
        //       {
        //           var taskDef = tasks.FirstOrDefault(t => t.TaskDefinitionId == taskId);
        //           if (taskDef == null) continue;

        //           var step = new WorkflowStep
        //           {
        //               Id = $"step_{stepSeq}",
        //               SeqNo = stepSeq,
        //               StepType = MapTaskTypeToStepType(taskDef.TaskType),
        //               Tasks = new List<WorkflowTask>
        //       {
        //           new WorkflowTask
        //           {
        //               Id = taskDef.TaskDefinitionId.ToString(),
        //               SeqNo = 1,
        //               TaskType = taskDef.TaskType,
        //               RuleSetId = taskDef.RuleReferenceId,
        //               Actions = BuildActionsForTask(taskDef)   // implement this separately
        //           }
        //       }
        //           };
        //           steps.Add(step);
        //           stepSeq++;
        //       }

        //       // 2. Handle parallel splits (tasks spawned from a gateway)
        //       var splits = transitions.Where(t => t.TransitionMode == "ParallelSplit").ToList();
        //       if (splits.Any())
        //       {
        //           var parallelStep = new WorkflowStep
        //           {
        //               Id = "step_parallel",
        //               SeqNo = stepSeq,
        //               StepType = StepType.Parallel,
        //               Tasks = new List<WorkflowTask>()
        //           };
        //           foreach (var split in splits.OrderBy(s => s.Priority))
        //           {
        //               var branchTask = tasks.FirstOrDefault(t => t.TaskDefinitionId == split.ToNodeId);
        //               if (branchTask != null)
        //               {
        //                   parallelStep.Tasks.Add(new WorkflowTask
        //                   {
        //                       Id = branchTask.TaskDefinitionId.ToString(),
        //                       SeqNo = split.Priority,
        //                       TaskType = branchTask.TaskType,
        //                       Actions = BuildActionsForTask(branchTask)
        //                   });
        //               }
        //           }
        //           if (parallelStep.Tasks.Any())
        //               steps.Add(parallelStep);
        //       }

        //       return steps;
        //   }

        //private StepType MapTaskTypeToStepType(string taskType)
        //{
        //    return taskType switch
        //    {
        //        "Gateway" => StepType.Decision,
        //        "Human Action Hub" => StepType.Decision,
        //        "System" or "Human" => StepType.Task,
        //        _ => StepType.Task
        //    };
        //}

        // You must implement this based on how actions are stored in your DB
        //private List<WorkflowAction> BuildActionsForTask(TaskDefinition taskDef)
        //{
        //    var actions = new List<WorkflowAction>();
        //    // Example: hardcoded for FWA case (replace with actual DB lookup)
        //    // For now, return an empty list; your mapper will then not add any activity,
        //    // but at least compilation succeeds.
        //    return actions;
        //}


        //private JsonArray BuildConnectionsFromTransitions(List<WorkflowTransition> transitions)
        //{
        //    var arr = new JsonArray();

        //    foreach (var tx in transitions.Where(t => t.TransitionMode != "Structure"))
        //    {
        //        string sourceId = GetActivityId(tx.FromNodeType, tx.FromNodeId);
        //        string sourcePort = GetSourcePortFromTransition(tx);
        //        string targetId = GetActivityId(tx.ToNodeType, tx.ToNodeId);
        //        string targetPort = "In";

        //        if (tx.TransitionMode == "ParallelSplit")
        //        {
        //            sourceId = $"Fork_{tx.FromNodeId}";
        //            sourcePort = "Done";
        //        }
        //        else if (tx.TransitionMode == "ParallelJoin")
        //        {
        //            targetId = $"Join_{tx.ToNodeId}";
        //            targetPort = "In";
        //        }

        //        arr.Add(MakeConnection(sourceId, sourcePort, targetId, targetPort));
        //    }
        //    return arr;
        //}

        //private string GetSourcePortFromTransition(WorkflowTransition tx)
        //{
        //    if (tx.TransitionPathType == "Success") return "Done";
        //    if (tx.TransitionPathType == "Event") return tx.EventDefinitionId.ToString();
        //    if (tx.TransitionPathType == "Custom" && tx.RuleReferenceType == "RULE")
        //        return "True";   // or "False" depending on rule outcome – you'd need actual rule mapping
        //    return "Done";
        //}

        // Wraps the root flowchart in the required "model" envelope
        //private string WrapInModel(Workflowdefinition definition, JsonObject root)
        //{
        //    var model = new JsonObject
        //    {
        //        ["definitionId"] = definition. DefinitionId.ToString(),
        //        ["name"] = definition.WorkflowName,
        //        ["description"] = definition.Description,
        //        ["isLatest"] = definition.Version?.IsPublished ?? true,
        //        ["isPublished"] = definition.Version?.IsPublished ?? true,
        //        ["variables"] = BuildVariables(definition),
        //        ["root"] = root
        //    };
        //    var wrapper = new JsonObject { ["model"] = model };
        //    return wrapper.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        //}

        // Builds all activity nodes (tasks/actions) from the definition
        //private JsonArray BuildAllActivities(Workflowdefinition definition)
        //{
        //    var arr = new JsonArray();
        //    foreach (var task in definition.Tasks)
        //    {
        //        foreach (var action in task.Actions)
        //        {
        //            arr.Add(BuildActionNode(action));
        //        }
        //    }
        //    // Also add FlowFork and FlowJoin nodes that are implied by transitions
        //    AddImplicitFlowNodes(definition, arr);
        //    return arr;
        //}

        //private void AddImplicitFlowNodes(Workflowdefinition definition, JsonArray arr)
        //{
        //    var splits = definition.Transitions.Where(t => t.TransitionMode == "ParallelSplit").ToList();
        //    var joins = definition.Transitions.Where(t => t.TransitionMode == "ParallelJoin").ToList();

        //    foreach (var split in splits)
        //    {
        //        // Add a FlowFork activity with id = $"Fork_{split.FromNodeId}"
        //        var forkId = $"Fork_{split.FromNodeId}";
        //        if (!arr.Any(a => a["id"]?.GetValue<string>() == forkId))
        //        {
        //            arr.Add(new JsonObject
        //            {
        //                ["id"] = forkId,
        //                ["type"] = "Elsa.FlowFork"
        //            });
        //        }
        //    }

        //    foreach (var join in joins)
        //    {
        //        var joinId = $"Join_{join.ToNodeId}";
        //        if (!arr.Any(a => a["id"]?.GetValue<string>() == joinId))
        //        {
        //            arr.Add(BuildFlowJoin(joinId, "WaitAll"));
        //        }
        //    }
        //}

        // Maps a node type + id to the actual activity ID used in the flowchart
        private string GetActivityId(string nodeType, int nodeId)
        {
            return nodeType switch
            {
                "WORKFLOW" => nodeId.ToString(),
                "TASK" => GetTaskActivityId(nodeId),
                "ACTION" => GetActionActivityId(nodeId),
                _ => nodeId.ToString()
            };
        }

        private string GetTaskActivityId(int taskDefId)
        {
            // You may have a mapping from TaskDefinitionId to the action ID.
            // In your DB, each task may produce one or more actions. For simplicity, use taskDefId as prefix.
            return $"Task_{taskDefId}";
        }

        private string GetActionActivityId(int actionDefId)
        {
            return $"Action_{actionDefId}";
        }

        // ════════════════════════════════════════════════════════════════════
        // Variables
        // ════════════════════════════════════════════════════════════════════

        //private static JsonArray BuildVariables(Workflowdefinition definition)
        //{
        //    var arr = new JsonArray
        //    {
        //        MakeVariable("var-tenantId",  "tenantId",  "String", definition.TenantId),
        //        MakeVariable("var-domainId",  "domainId",  "String", definition.DomainId)
        //    };

        //    // Any extra variables declared on steps (Decision rule outputs, etc.)
        //    foreach (var step in definition.Steps ?? Enumerable.Empty<WorkflowStep>())
        //    {
        //        foreach (var task in step.Tasks ?? Enumerable.Empty<WorkflowTask>())
        //        {
        //            foreach (var action in task.Actions ?? Enumerable.Empty<WorkflowAction>())
        //            {
        //                if (action.ActionType == "EvaluateRule")
        //                {
        //                    var cfg = action.GetConfig<EvaluateRuleConfig>();
        //                    arr.Add(MakeVariable(
        //                        $"var-{cfg.OutputVariable}",
        //                        cfg.OutputVariable, "String", null));
        //                }
        //                else if (action.ActionType == "SetVariable")
        //                {
        //                    var cfg = action.GetConfig<SetVariableConfig>();
        //                    arr.Add(MakeVariable(cfg.VariableId, cfg.VariableName, cfg.VariableTypeName, null));
        //                }
        //            }
        //        }
        //    }

        //    return arr;
        //}

        private static JsonObject MakeVariable(string id, string name, string typeName, string? value)
        {
            var v = new JsonObject
            {
                ["id"] = id,
                ["name"] = name,
                ["typeName"] = typeName,
                ["isArray"] = false,
                ["value"] = value is null ? null : JsonValue.Create(value),
                ["storageDriverTypeName"] =
                    "Elsa.Workflows.WorkflowInstanceStorageDriver, Elsa.Workflows.Core"
            };
            return v;
        }

        public List<WorkflowStep> BuildStepsFromTasksAndTransitions(
    List<TaskDefinition> tasks,
    List<WorkflowTransition> transitions)
        {
            var steps = new List<WorkflowStep>();
            var sequentialTransitions = transitions
                .Where(t => t.TransitionMode == "Sequential" && t.TransitionPathType == "Success")
                .OrderBy(t => t.Priority)
                .ToList();

            // Get ordered task IDs
            var orderedTaskIds = new List<int>();
            var currentTaskId = sequentialTransitions.FirstOrDefault()?.FromNodeId;
            while (currentTaskId != null)
            {
                orderedTaskIds.Add(currentTaskId.Value);
                var next = sequentialTransitions.FirstOrDefault(t => t.FromNodeId == currentTaskId);
                currentTaskId = next?.ToNodeId;
            }

            int stepSeq = 1;
            foreach (var taskId in orderedTaskIds)
            {
                var taskDef = tasks.First(t => t.TaskDefinitionId == taskId);
                var step = new WorkflowStep
                {
                    StepId = $"step_{stepSeq}",
                    SeqNo = stepSeq,
                    StepType = MapTaskTypeToStepType(taskDef.TaskType),
                    Tasks = new List<WorkflowTask>
            {
                new WorkflowTask
                {
                    Id = taskDef.TaskDefinitionId.ToString(),
                    SeqNo = 1,
                    TaskType = taskDef.TaskType,
                    RuleSetId = taskDef.RuleReferenceId,
                    Actions = BuildActionsForTask(taskDef)   // you need to map ActionDefinitions
                }
            }
                };
                steps.Add(step);
                stepSeq++;
            }

            // Handle ParallelSplit groups: group tasks that are split from the same gateway
            var splits = transitions.Where(t => t.TransitionMode == "ParallelSplit").ToList();
            if (splits.Any())
            {
                var parallelStep = new WorkflowStep
                {
                    StepId = $"step_parallel",
                    SeqNo = stepSeq,
                    StepType = StepType.Parallel,
                    Tasks = new List<WorkflowTask>()
                };
                foreach (var split in splits)
                {
                    var branchTaskDef = tasks.First(t => t.TaskDefinitionId == split.ToNodeId);
                    parallelStep.Tasks.Add(new WorkflowTask
                    {
                        Id = branchTaskDef.TaskDefinitionId.ToString(),
                        SeqNo = split.Priority,
                        TaskType = branchTaskDef.TaskType,
                        Actions = BuildActionsForTask(branchTaskDef)
                    });
                }
                steps.Add(parallelStep);
            }

            return steps;
        }

        private List<WorkflowAction> BuildActionsForTask(TaskDefinition taskDef)
        {
            // Assuming you have a collection of ActionDefinition linked to the task
            // via a junction table or by parsing the transition's EventDefinitionId, etc.
            // For simplicity, here's a hardcoded example matching the FWA case:
            var actions = new List<WorkflowAction>();

            if (taskDef.TaskName == "Case Initiated")
            {
                actions.Add(new WorkflowAction
                {
                    Id = "SaveInputs",
                    SeqNo = 1,
                    ActionType = "RunJavaScript",
                    ConfigJson = JsonSerializer.Serialize(new RunJavaScriptConfig
                    {
                        Script = "setVariable('entityId', getInput('entityId'));"
                    })
                });

                actions.Add(new WorkflowAction
                {
                    Id = "CaseInitiated",
                    SeqNo = 2,
                    ActionType = "ExecuteWorkflow",
                    ConfigJson = JsonSerializer.Serialize(new ExecuteWorkflowConfig
                    {
                        WorkflowDefinitionId = "HttpWithRetryWorkflow"
                    })
                });
                // ... etc
            }
            // Handle other tasks (Preliminary Review, SME Clinical Review, etc.)
            return actions;
        }
        private StepType MapTaskTypeToStepType(string taskType)
        {
            return taskType switch
            {
                "Gateway" => StepType.Decision,
                "Human Action Hub" => StepType.Decision,   // Optional Action Decision is a decision step
                "System" or "Human" => StepType.Task,
                _ => StepType.Task
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // Activities
        // ════════════════════════════════════════════════════════════════════

        private JsonArray BuildActivities(Workflowdefinition definition)
        {
            var arr = new JsonArray();

            foreach (var step in definition.Steps!.OrderBy(s => s.SeqNo))
            {
                foreach (var node in ConvertStep(step))
                    arr.Add(node);
            }

            return arr;
        }

        private IEnumerable<JsonObject> ConvertStep(WorkflowStep step)
        {
            return step.StepType switch
            {
                StepType.Wait => new[] { BuildDelay(step) },
                StepType.Parallel => BuildParallelGroup(step),
                StepType.Decision => BuildDecisionGroup(step),   // ← replaces single BuildFlowDecision
                _ => BuildTaskActivities(step)
            };
        }
        // ── Decision step — emits FlowDecision + True/False branch activities ────

        private IEnumerable<JsonObject> BuildDecisionGroup(WorkflowStep step)
        {
            var nodes = new List<JsonObject>();

            // Collect all actions across all tasks in order
            var allActions = step.Tasks
                .OrderBy(t => t.SeqNo)
                .SelectMany(t => t.Actions.OrderBy(a => a.SeqNo))
                .ToList();

            if (allActions.Count == 0)
            {
                nodes.Add(BuildNoOp(step.StepId));
                return nodes;
            }

            // Emit all activities as flat nodes — connections handle True/False routing
            foreach (var action in allActions)
                nodes.Add(BuildActionNode(action));

            return nodes;
        }

        // ── Task step ────────────────────────────────────────────────────────

        private IEnumerable<JsonObject> BuildTaskActivities(WorkflowStep step)
        {
            var nodes = new List<JsonObject>();

            foreach (var task in step.Tasks.OrderBy(t => t.SeqNo))
            {
                foreach (var action in task.Actions.OrderBy(a => a.SeqNo))
                {
                    nodes.Add(BuildActionNode(action));
                }
            }

            return nodes.Count > 0 ? nodes : new List<JsonObject> { BuildNoOp(step.StepId) };
        }

        private JsonObject BuildActionNode(WorkflowAction action)
        {
            return action.ActionType switch
            {
                "RunJavaScript" => BuildRunJavaScript(action),
                "ExecuteWorkflow" => BuildExecuteWorkflow(action),
                "HttpCall" => BuildHttpCall(action),
                "SetVariable" => BuildSetVariable(action),
                "WriteLine" => BuildWriteLine(action),
                "WaitForEvent" => BuildWaitForEvent(action),
                "FlowDecision" => BuildFlowDecisionFromAction(action),
                "EvaluateRule" => BuildEvaluateRule(action),
                "Finish" => BuildFinish(action),   // new
                _ => BuildNoOp(action.Id)
            };
        }

        // ── Individual activity builders ─────────────────────────────────────

        private JsonObject BuildRunJavaScript(WorkflowAction action)
        {
            var cfg = action.GetConfig<RunJavaScriptConfig>();
            return new JsonObject
            {
                ["id"] = action.Id,
                ["type"] = "Elsa.RunJavaScript",
                ["script"] = TypedExpression("String", "JavaScript", cfg.Script)
            };
        }

        private JsonObject BuildExecuteWorkflow(WorkflowAction action)
        {
            var cfg = action.GetConfig<ExecuteWorkflowConfig>();
            return new JsonObject
            {
                ["id"] = action.Id,
                ["type"] = "Elsa.ExecuteWorkflow",
                ["workflowDefinitionId"] = TypedExpression("String", "Literal", cfg.WorkflowDefinitionId),
                ["input"] = TypedExpression(cfg.InputTypeName, "JavaScript", cfg.InputExpression),
                ["waitForCompletion"] = TypedExpression("Boolean", "Literal", cfg.WaitForCompletion)
            };
        }

        /// <summary>
        /// HttpCall config → ExecuteWorkflow targeting HttpWithRetryWorkflow.
        /// Matches the pattern used throughout the FWA example.
        /// </summary>
        private JsonObject BuildHttpCall(WorkflowAction action)
        {
            var cfg = action.GetConfig<HttpConfig>();

            var bodyJson = cfg.Body is not null
                ? cfg.Body.ToString()
                : "{}";

            var jsExpr =
                $"({{ url: '{cfg.Url}', method: '{cfg.Method}', " +
                $"body: {bodyJson}, " +
                $"contentType: '{cfg.ContentType}', " +
                $"maxRetries: {cfg.MaxRetries}, " +
                $"retryDelay: '{cfg.RetryDelay}' }})";

            return new JsonObject
            {
                ["id"] = action.Id,
                ["type"] = "Elsa.ExecuteWorkflow",
                ["workflowDefinitionId"] = TypedExpression("string", "Literal", "HttpWithRetryWorkflow"),
                ["input"] = TypedExpression("ObjectDictionary", "JavaScript", jsExpr),
                ["waitForCompletion"] = TypedExpression("Boolean", "Literal", true)
            };
        }


        private JsonObject BuildSetVariable(WorkflowAction action)
        {
            var cfg = action.GetConfig<SetVariableConfig>();

            return new JsonObject
            {
                ["id"] = action.Id,
                ["type"] = "Elsa.SetVariable",
                ["variable"] = new JsonObject
                {
                    ["id"] = cfg.VariableId,
                    ["name"] = cfg.VariableName,
                    ["typeName"] = cfg.VariableTypeName,
                    ["memoryReference"] = new JsonObject { ["id"] = cfg.VariableId }
                },
                ["value"] = TypedExpression(
                    cfg.VariableTypeName,
                    cfg.Value.ExprType == ExpressionType.JavaScript ? "JavaScript" : "Literal",
                    cfg.Value.Value)
            };
        }

        private JsonObject BuildWriteLine(WorkflowAction action)
        {
            var cfg = action.GetConfig<WriteLineConfig>();
            var exprType = cfg.Text.ExprType == ExpressionType.JavaScript ? "JavaScript" : "Literal";

            return new JsonObject
            {
                ["id"] = action.Id,
                ["type"] = "Elsa.WriteLine",
                ["text"] = TypedExpression("String", exprType, cfg.Text.Value)
            };
        }

        private JsonObject BuildWaitForEvent(WorkflowAction action)
        {
            var cfg = action.GetConfig<WaitForEventConfig>();
            return new JsonObject
            {
                ["id"] = action.Id,
                ["type"] = "Elsa.Event",
                ["eventName"] = TypedExpression("String", "Literal", cfg.EventName)
            };
        }

        private JsonObject BuildFlowDecisionFromAction(WorkflowAction action)
        {
            var cfg = action.GetConfig<FlowDecisionConfig>();
            return new JsonObject
            {
                ["id"] = action.Id,
                ["type"] = "Elsa.FlowDecision",
                ["condition"] = TypedExpression("Boolean", "JavaScript", cfg.ConditionExpression)
            };
        }

        private JsonObject BuildEvaluateRule(WorkflowAction action)
        {
            var cfg = action.GetConfig<EvaluateRuleConfig>();
            return new JsonObject
            {
                ["id"] = action.Id,
                ["type"] = "MyApp.EvaluateRuleActivity",
                ["ruleWorkflow"] = TypedExpression("String", "Literal", cfg.RuleWorkflow),
                ["outputVariable"] = TypedExpression("String", "Literal", cfg.OutputVariable),
                ["waitForCompletion"] = TypedExpression("Boolean", "Literal", cfg.WaitForCompletion)
            };
        }

        // ── Delay (Wait step) ────────────────────────────────────────────────

        private JsonObject BuildDelay(WorkflowStep step)
        {
            var minutes = step.TimeoutMinutes ?? 5;
            var ts = $"00:{minutes:D2}:00";

            return new JsonObject
            {
                ["id"] = step.StepId,
                ["type"] = "Elsa.Delay",
                ["timeSpan"] = TypedExpression("String", "Literal", ts)
            };
        }

        // ── Parallel (Fork + per-branch decisions + Join) ────────────────────

        private IEnumerable<JsonObject> BuildParallelGroup(WorkflowStep step)
        {
            var nodes = new List<JsonObject>();

            // FlowFork — fan-out node
            nodes.Add(new JsonObject
            {
                ["id"] = step.StepId,
                ["type"] = "Elsa.FlowFork"
            });

            // Each task becomes its own branch activities
            foreach (var task in step.Tasks.OrderBy(t => t.SeqNo))
            {
                foreach (var action in task.Actions.OrderBy(a => a.SeqNo))
                    nodes.Add(BuildActionNode(action));
            }

            // FlowJoin — fan-in node
            var joinId = $"{step.StepId}_Join";
            nodes.Add(BuildFlowJoin(joinId, "WaitAll"));

            return nodes;
        }

        private JsonObject BuildFlowJoin(string id, string mode)
        {
            return new JsonObject
            {
                ["id"] = id,
                ["nodeId"] = $"Workflow1:Flowchart1:{id}",
                ["name"] = id,
                ["type"] = "Elsa.FlowJoin",
                ["version"] = 1,
                ["customProperties"] = new JsonObject
                {
                    ["canStartWorkflow"] = false,
                    ["runAsynchronously"] = false
                },
                ["metadata"] = new JsonObject(),
                ["mode"] = new JsonObject
                {
                    ["typeName"] = "Elsa.Workflows.Activities.Flowchart.Models.FlowJoinMode, Elsa.Workflows.Core",
                    ["expression"] = new JsonObject
                    {
                        ["type"] = "Literal",
                        ["value"] = mode
                    }
                }
            };
        }

        private JsonObject BuildFinish(WorkflowAction action)
        {
            var displayText = action.ActionName ?? "End";
            return new JsonObject
            {
                ["id"] = action.Id,
                ["type"] = "Elsa.Finish",
                ["name"] = action.ActionName ?? "End",
                ["metadata"] = new JsonObject { ["displayText"] = displayText }
            };
        }
        // ── Decision step ────────────────────────────────────────────────────

        private JsonObject BuildFlowDecision(WorkflowStep step)
        {
            var condition = step.Tasks.FirstOrDefault()?.RuleSetId ?? "true";

            return new JsonObject
            {
                ["id"] = step.StepId,
                ["type"] = "Elsa.FlowDecision",
                ["condition"] = TypedExpression("Boolean", "JavaScript", condition)
            };
        }

        // ── NoOp fallback ────────────────────────────────────────────────────

        private static JsonObject BuildNoOp(string id) =>
            new JsonObject { ["id"] = id, ["type"] = "Elsa.NoOp" };

        // ════════════════════════════════════════════════════════════════════
        // Connections
        // ════════════════════════════════════════════════════════════════════

        //private JsonArray BuildConnections(Workflowdefinition definition)
        //{
        //    var arr = new JsonArray();

        //    foreach (var tx in definition.Transitions.Where(t => t.TransitionMode != "Structure"))
        //    {
        //        string sourceId = GetActivityId(tx.FromNodeType, tx.FromNodeId);
        //        string sourcePort = GetSourcePort(tx);
        //        string targetId = GetActivityId(tx.ToNodeType, tx.ToNodeId);
        //        string targetPort = "In";

        //        // Adjust for forks/joins
        //        if (tx.TransitionMode == "ParallelSplit")
        //        {
        //            // Source is the Fork node, not the original task
        //            sourceId = $"Fork_{tx.FromNodeId}";
        //            sourcePort = "Done";
        //        }
        //        else if (tx.TransitionMode == "ParallelJoin")
        //        {
        //            // Target is the Join node
        //            targetId = $"Join_{tx.ToNodeId}";
        //            targetPort = "In";
        //        }

        //        arr.Add(MakeConnection(sourceId, sourcePort, targetId, targetPort));
        //    }
        //    return arr;
        //}

        private string GetSourcePort(WorkflowTransition tx)
        {
            if (tx.TransitionPathType == "Success") return "Done";
            if (tx.TransitionPathType == "Event") return tx.EventDefinitionId?.ToString() ?? "Done";
            if (tx.TransitionPathType == "Custom" && tx.RuleReferenceType == "RULE")
                return "True";   // or "False" – you would need rule outcome mapping; default to True
            return "Done";
        }
        /// <summary>
        /// After a Decision step completes (True or False branch), both terminals
        /// must connect forward to the next step's entry activity.
        /// Convention mirrors WireDecisionStep:
        ///   allActions[1] = True branch terminal
        ///   allActions[2] = False branch terminal
        /// </summary>
        private void WireDecisionNextStep(WorkflowStep step, string nextEntryId, JsonArray arr)
        {
            var allActions = step.Tasks
                .OrderBy(t => t.SeqNo)
                .SelectMany(t => t.Actions.OrderBy(a => a.SeqNo))
                .ToList();

            // True branch terminal → next step
            if (allActions.Count > 1)
                arr.Add(MakeConnection(allActions[1].Id, "Done", nextEntryId, "In"));

            // False branch terminal → next step
            if (allActions.Count > 2)
            {
                if (allActions[2].HasConnection == true)
                    arr.Add(MakeConnection(allActions[2].Id, "Done", nextEntryId, "In"));
            }
                

            // Edge case: only a gate with no branches — gate itself connects forward
            if (allActions.Count == 1)
                arr.Add(MakeConnection(allActions[0].Id, "Done", nextEntryId, "In"));
        }

        /// <summary>
        /// Wires a Decision step's internal True/False port connections.
        /// Convention:
        ///   action seqNo 1 = FlowDecision gate
        ///   action seqNo 2 = True branch activity  (FlowDecision → True → seqNo2)
        ///   action seqNo 3 = False branch activity (FlowDecision → False → seqNo3)
        ///   both seqNo2 and seqNo3 are terminal — no further internal wiring
        /// </summary>
        private void WireDecisionStep(WorkflowStep step, JsonArray arr)
        {
            var allActions = step.Tasks
                .OrderBy(t => t.SeqNo)
                .SelectMany(t => t.Actions.OrderBy(a => a.SeqNo))
                .ToList();

            if (allActions.Count < 1) return;

            var gateAction = allActions[0];   // FlowDecision
            var trueAction = allActions.Count > 1 ? allActions[1] : null;
            var falseAction = allActions.Count > 2 ? allActions[2] : null;

            if (trueAction is not null)
                arr.Add(MakeConnection(gateAction.Id, "True", trueAction.Id, "In"));

            if (falseAction is not null)
                arr.Add(MakeConnection(gateAction.Id, "False", falseAction.Id, "In"));
        }

        /// <summary>
        /// Wires a Parallel step: Fork → Done → each branch entry,
        /// each branch exit → Join, and sequential wiring within each branch.
        /// </summary>
        private void WireParallelStep(WorkflowStep step, JsonArray arr)
        {
            var joinId = $"{step.StepId}_Join";

            foreach (var task in step.Tasks.OrderBy(t => t.SeqNo))
            {
                var actions = task.Actions.OrderBy(a => a.SeqNo).ToList();
                if (actions.Count == 0) continue;

                var forkId = step.StepId;
                var outerGate = actions[0];          // always FlowDecision
                var branchDone = actions[^1];        // always the terminal WriteLine

                // Fork → outer gate
                arr.Add(MakeConnection(forkId, "Done", outerGate.Id, "In"));

                // Outer gate False → branch done (short-circuit when condition not met)
                arr.Add(MakeConnection(outerGate.Id, "False", branchDone.Id, "In"));

                if (actions.Count == 1)
                {
                    // Only a gate — it IS the terminal
                    arr.Add(MakeConnection(outerGate.Id, "Done", joinId, "In"));
                    continue;
                }

                if (actions.Count == 2)
                {
                    // Gate + terminal only
                    arr.Add(MakeConnection(outerGate.Id, "True", branchDone.Id, "In"));
                    arr.Add(MakeConnection(branchDone.Id, "Done", joinId, "In"));
                    continue;
                }

                // Find if there is an inner FlowDecision gate (second FlowDecision in the list)
                // It sits before the failure log and branch-done
                int innerGateIndex = -1;
                for (int j = actions.Count - 2; j >= 1; j--)
                {
                    if (actions[j].ActionType == "FlowDecision")
                    {
                        innerGateIndex = j;
                        break;
                    }
                }

                if (innerGateIndex > 0)
                {
                    // ── Complex branch: OuterGate → work chain → InnerGate → failLog/done ──
                    //
                    // Layout: [0:outerGate] [1..innerGateIndex-1: work chain]
                    //         [innerGateIndex: innerGate]
                    //         [innerGateIndex+1: failLog]   (if exists between innerGate and branchDone)
                    //         [last: branchDone]

                    var innerGate = actions[innerGateIndex];
                    var failLog = innerGateIndex + 1 < actions.Count - 1
                                    ? actions[innerGateIndex + 1]
                                    : null;

                    // Outer gate True → first work action
                    arr.Add(MakeConnection(outerGate.Id, "True", actions[1].Id, "In"));

                    // Sequential wiring through work chain: [1 .. innerGateIndex-1]
                    for (int j = 1; j < innerGateIndex; j++)
                        arr.Add(MakeConnection(actions[j].Id, "Done", actions[j + 1].Id, "In"));

                    // Inner gate True → branch done (success path)
                    arr.Add(MakeConnection(innerGate.Id, "True", branchDone.Id, "In"));

                    if (failLog != null)
                    {
                        // Inner gate False → fail log → branch done
                        arr.Add(MakeConnection(innerGate.Id, "False", failLog.Id, "In"));
                        arr.Add(MakeConnection(failLog.Id, "Done", branchDone.Id, "In"));
                    }
                    else
                    {
                        // Inner gate False → branch done directly
                        arr.Add(MakeConnection(innerGate.Id, "False", branchDone.Id, "In"));
                    }
                }
                else
                {
                    // ── Simple branch: OuterGate → sequential work → BranchDone ──
                    arr.Add(MakeConnection(outerGate.Id, "True", actions[1].Id, "In"));

                    for (int j = 1; j < actions.Count - 1; j++)
                        arr.Add(MakeConnection(actions[j].Id, "Done", actions[j + 1].Id, "In"));
                }

                // Branch done → Join (applies to all branch shapes)
                arr.Add(MakeConnection(branchDone.Id, "Done", joinId, "In"));
            }
        }
        /// <summary>First activity ID the flowchart should connect INTO for this step.</summary>
        private string GetStepEntryId(WorkflowStep step)
        {
            // For Decision: entry is the FlowDecision gate (first action)
            // For Parallel: entry is the Fork node (step.Id)
            // For Wait:     entry is the Delay node (step.Id)
            if (step.StepType is StepType.Wait or StepType.Parallel)
                return step.StepId;

            // Decision and Task: entry is first action
            return step.Tasks
                .OrderBy(t => t.SeqNo)
                .SelectMany(t => t.Actions.OrderBy(a => a.SeqNo))
                .FirstOrDefault()?.Id ?? step.StepId;
        }

        private string GetStepExitId(WorkflowStep step)
        {
            if (step.StepType == StepType.Wait)
                return step.StepId;

            if (step.StepType == StepType.Parallel)
                return $"{step.StepId}_Join";

            if (step.StepType == StepType.Decision)
                // Exit is handled explicitly via WireDecisionNextStep;
                // return the gate ID only as a non-null placeholder — never used for MakeConnection.
                return step.Tasks
                    .OrderBy(t => t.SeqNo)
                    .SelectMany(t => t.Actions.OrderBy(a => a.SeqNo))
                    .FirstOrDefault()?.Id ?? step.StepId;

            return step.Tasks
                .OrderBy(t => t.SeqNo)
                .SelectMany(t => t.Actions.OrderBy(a => a.SeqNo))
                .LastOrDefault()?.Id ?? step.StepId;
        }

        private static JsonObject MakeConnection(
            string sourceActivity, string sourcePort,
            string targetActivity, string targetPort)
        {
            return new JsonObject
            {
                ["source"] = new JsonObject { ["activity"] = sourceActivity, ["port"] = sourcePort },
                ["target"] = new JsonObject { ["activity"] = targetActivity, ["port"] = targetPort }
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // Expression helper — matches Elsa's typed-expression envelope
        // ════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Builds: { "typeName": "...", "expression": { "type": "...", "value": ... } }
        /// </summary>
        private static JsonObject TypedExpression(string typeName, string exprType, object? value)
        {
            JsonNode? jsonValue = value switch
            {
                null => null,
                bool b => JsonValue.Create(b),
                int n => JsonValue.Create(n),
                string s => JsonValue.Create(s),
                _ => JsonValue.Parse(JsonSerializer.Serialize(value, _jsonOpts))
            };

            return new JsonObject
            {
                ["typeName"] = typeName,
                ["expression"] = new JsonObject
                {
                    ["type"] = exprType,
                    ["value"] = jsonValue
                }
            };
        }
        //  }



        public string ConvertToElsaJson(Workflowdefinition definition)
        {
            // 1. Build all activities from Steps
            var activities = BuildAllActivities(definition);

            // 2. Build connections from the Step structure (not from transitions)
            var connections = BuildConnections(definition);   // ← changed

            var root = new JsonObject
            {
                ["id"] = $"{definition.DefinitionId}_Flowchart",
                ["type"] = "Elsa.Flowchart",
                ["activities"] = activities,
                ["connections"] = connections
            };

            return WrapInModel(definition, root);
        }

        // ---------- Activity Builders ----------
        private JsonArray BuildAllActivities(Workflowdefinition definition)
        {
            var arr = new JsonArray();

            // Add Cron trigger if defined
            if (!string.IsNullOrWhiteSpace(definition.CronExpression))
            {
                arr.Add(BuildCron(definition.CronExpression));
            }
            foreach (var step in definition.Steps.OrderBy(s => s.SeqNo))
            {
                if (step.StepType == StepType.Parallel)
                {
                    // Add the FlowFork node
                    arr.Add(BuildFlowFork(step.StepId));

                    // Add all actions from all tasks in this parallel step
                    foreach (var task in step.Tasks.OrderBy(t => t.SeqNo))
                        foreach (var action in task.Actions.OrderBy(a => a.SeqNo))
                            arr.Add(BuildActionNode(action));

                    // Add the FlowJoin node
                    arr.Add(BuildFlowJoin($"{step.StepId}_Join", "WaitAll"));
                }
                else
                {
                    // Regular step: just add all actions
                    foreach (var task in step.Tasks.OrderBy(t => t.SeqNo))
                        foreach (var action in task.Actions.OrderBy(a => a.SeqNo))
                            arr.Add(BuildActionNode(action));
                }
            }
            return arr;
        }

        private JsonObject BuildFlowFork(string id)
        {
            return new JsonObject
            {
                ["id"] = id,
                ["type"] = "Elsa.FlowFork"
            };
        }

        private void AddImplicitFlowNodes(Workflowdefinition definition, JsonArray arr)
        {
            var parallelSteps = definition.Steps.Where(s => s.StepType == StepType.Parallel).ToList();

            foreach (var step in parallelSteps)
            {
                // Fork node
                var forkId = step.StepId;
                if (!arr.Any(a => a["SStepIdid"]?.GetValue<string>() == forkId))
                {
                    arr.Add(new JsonObject { ["StepId"] = forkId, ["type"] = "Elsa.FlowFork" });
                }

                // Join node
                var joinId = $"{step.StepId}_Join";
                if (!arr.Any(a => a["StepId"]?.GetValue<string>() == joinId))
                {
                    arr.Add(BuildFlowJoin(joinId, "WaitAll"));
                }
            }
        }

        // ---------- Connection Builders ----------
        private JsonArray BuildConnectionsFromTransitions(List<WorkflowTransition> transitions)
        {
            var arr = new JsonArray();

            foreach (var tx in transitions.Where(t => t.TransitionMode != "Structure"))
            {
                string sourceId = GetActivityId(tx.FromNodeType, tx.FromNodeId);
                string sourcePort = GetSourcePortFromTransition(tx);
                string targetId = GetActivityId(tx.ToNodeType, tx.ToNodeId);
                string targetPort = "In";

                if (tx.TransitionMode == "ParallelSplit")
                {
                    sourceId = $"Fork_{tx.FromNodeId}";
                    sourcePort = "Done";
                }
                else if (tx.TransitionMode == "ParallelJoin")
                {
                    targetId = $"Join_{tx.ToNodeId}";
                    targetPort = "In";
                }

                arr.Add(MakeConnection(sourceId, sourcePort, targetId, targetPort));
            }

            return arr;
        }

        private string GetActivityId(string nodeType, string nodeId)
        {
            // Since your IDs are strings (e.g., "SaveInputs", "Task_001"), we return them directly.
            // For WORKFLOW, we use the definition ID.
            return nodeType switch
            {
                "WORKFLOW" => nodeId,   // definition ID
                _ => nodeId             // task/action ID as given
            };
        }

        private string GetSourcePortFromTransition(WorkflowTransition tx)
        {
            if (tx.TransitionPathType == "Success") return "Done";
            if (tx.TransitionPathType == "Event") return tx.EventDefinitionId?.ToString() ?? "Done";
            if (tx.TransitionPathType == "Custom" && tx.RuleReferenceType == "RULE") return "True";
            return "Done";
        }

        // ---------- Helpers ----------
        private string WrapInModel(Workflowdefinition definition, JsonObject root)
        {
            var model = new JsonObject
            {
                ["definitionId"] = definition.DefinitionId,
                ["name"] = definition.WorkflowName,
                ["description"] = definition.Description,
                ["isLatest"] = definition.IsPublished,
                ["isPublished"] = definition.IsPublished,
                ["version"] = definition.VersionNo,
                ["variables"] = BuildVariables(definition),
                ["root"] = root
            };
            var wrapper = new JsonObject { ["model"] = model };
            return wrapper.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        }

        private JsonArray BuildVariables(Workflowdefinition definition)
        {
            var arr = new JsonArray();
            arr.Add(MakeVariable("var-tenantId", "tenantId", "String", definition.TenantId));
            arr.Add(MakeVariable("var-domainId", "domainId", "String", definition.DomainId));

            foreach (var step in definition.Steps ?? Enumerable.Empty<WorkflowStep>())
            {
                foreach (var task in step.Tasks ?? Enumerable.Empty<WorkflowTask>())
                {
                    foreach (var action in task.Actions ?? Enumerable.Empty<WorkflowAction>())
                    {
                        if (action.ActionType == "EvaluateRule")
                        {
                            var cfg = action.GetConfig<EvaluateRuleConfig>();
                            arr.Add(MakeVariable($"var-{cfg.OutputVariable}", cfg.OutputVariable, "String", null));
                        }
                        else if (action.ActionType == "SetVariable")
                        {
                            var cfg = action.GetConfig<SetVariableConfig>();
                            arr.Add(MakeVariable(cfg.VariableId, cfg.VariableName, cfg.VariableTypeName, null));
                        }
                    }
                }
            }
            return arr;
        }
        private JsonArray BuildConnections(Workflowdefinition definition)
        {
            var arr = new JsonArray();
            var steps = definition.Steps.OrderBy(s => s.SeqNo).ToList();
            string? prevExitId = null;
            bool prevWasDecision = false;
            WorkflowStep? prevDecisionStep = null;


            // Connect Cron to first step if Cron exists
            if (!string.IsNullOrWhiteSpace(definition.CronExpression) && steps.Any())
            {
                var firstEntry = GetStepEntryId(steps.First());
                arr.Add(MakeConnection("trigger-Cron", "Done", firstEntry, "In"));
            }

            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps[i];
                var entryId = GetStepEntryId(step);
                var exitId = GetStepExitId(step);

                if (prevWasDecision && prevDecisionStep != null)
                {
                    WireDecisionNextStep(prevDecisionStep, entryId, arr);
                }
                else if (prevExitId != null)
                {
                    arr.Add(MakeConnection(prevExitId, "Done", entryId, "In"));
                }

                if (step.StepType == StepType.Decision)
                {
                    WireDecisionStep(step, arr);
                    prevWasDecision = true;
                    prevDecisionStep = step;
                }
                else if (step.StepType == StepType.Parallel)
                {
                    WireParallelStep(step, arr);
                    prevWasDecision = false;
                    prevDecisionStep = null;
                }
                else
                {
                    prevWasDecision = false;
                    prevDecisionStep = null;
                }

                prevExitId = exitId;
            }
            return arr;
        }

        private JsonObject BuildCron(string cronExpression)
        {
            return new JsonObject
            {
                ["id"] = "trigger-Cron",
                ["type"] = "Elsa.Cron",
                ["name"] = "Scheduled Trigger",
                ["version"] = 1,
                ["cronExpression"] = TypedExpression("String", "Literal", cronExpression),
                ["customProperties"] = new JsonObject { ["canStartWorkflow"] = true }
            };
        }
    }

}
//using EnterpriseWorkflow.Domain.Configurations;
//using EnterpriseWorkflow.Domain.Entities.Workflow;
//using EnterpriseWorkflow.Domain.Enums;
//using System.Text.Json;
//using System.Text.Json.Serialization;
//using System.Threading.Tasks;

//namespace EnterpriseWorkflow.Elsa.Mapping
//{
//    // EnterpriseWorkflow.Elsa/Mapping/EnterpriseToElsaMapper.cs
//    public sealed class EnterpriseToElsaMapper
//    {
//        /// <summary>
//        /// Converts enterprise workflow definition to Elsa 3.x JSON format
//        /// </summary>
//        public string ConvertToElsaJson(Workflowdefinition definition)
//        {
//            var flowchart = new ElsaFlowchart
//            {
//                Id = Guid.NewGuid().ToString(),
//                Activities = new List<ElsaActivity>(),
//                Connections = new List<ElsaConnection>()
//            };

//            ElsaActivity? previousActivity = null;

//            // Convert each step to Elsa activities
//            foreach (var step in definition.Steps.OrderBy(s => s.SeqNo))
//            {
//                var activity = ConvertStepToElsaActivity(step);
//                flowchart.Activities.Add(activity);

//                // Create connection from previous activity
//                if (previousActivity != null)
//                {
//                    flowchart.Connections.Add(new ElsaConnection
//                    {
//                        Source = new ElsaConnectionEndpoint { Activity = previousActivity.Id, Port = "Done" },
//                        Target = new ElsaConnectionEndpoint { Activity = activity.Id, Port = "In" }
//                    });
//                }

//                previousActivity = activity;
//            }

//            // Build complete workflow definition JSON
//            var elsaWorkflow = new ElsaWorkflowDefinition
//            {
//                DefinitionId = definition.DefinitionId,
//                Name = definition.Name,
//                Description = definition.Description,
//                Version = (int)definition.Version,
//                Root = flowchart,
//                Variables = BuildVariables(definition),
//                Inputs = new List<ElsaInputDefinition>
//                {
//                    new() { Name = "Payload", Type = "Object" }
//                },
//                Outputs = new List<ElsaOutputDefinition>
//                {
//                    new() { Name = "Result", Type = "Object" }
//                }
//            };

//            // Serialize with camelCase (Elsa's expected format)
//            var options = new JsonSerializerOptions
//            {
//                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
//            };

//            return JsonSerializer.Serialize(elsaWorkflow, options);
//        }

//        private ElsaActivity ConvertStepToElsaActivity(WorkflowStep step)
//        {
//            return step.StepType switch
//            {
//                StepType.Task => ConvertTaskStep(step),
//                StepType.Wait => CreateDelayActivity(step),
//                StepType.Parallel => CreateParallelActivity(step),
//                StepType.Decision => CreateDecisionActivity(step),
//                _ => ConvertTaskStep(step)
//            };
//        }

//        private ElsaActivity ConvertTaskStep(WorkflowStep step)
//        {
//            // For task steps, create a sequence of activities
//            var activities = new List<ElsaActivity>();

//            foreach (var task in step.Tasks.OrderBy(t => t.SeqNo))
//            {
//                foreach (var action in task.Actions.OrderBy(a => a.SeqNo))
//                {
//                    activities.Add(CreateActionActivity(action));
//                }
//            }

//            // If multiple activities in a step, wrap in a Sequence
//            if (activities.Count > 1)
//            {
//                return new ElsaSequenceActivity
//                {
//                    Id = step.Id,
//                    NodeId = $"Activity:{step.Id}",
//                    Activities = activities,
//                    Connections = CreateSequentialConnections(activities)
//                };
//            }

//            return activities.FirstOrDefault() ?? CreateNoOpActivity(step);
//        }

//        private ElsaActivity CreateActionActivity(WorkflowAction action)
//        {
//            return action.ActionType switch
//            {
//                "SendEmail" => new ElsaSendEmailActivity
//                {
//                    Id = action.Id,
//                    NodeId = $"Activity:{action.Id}",
//                    To = new ElsaExpression("Literal", action.GetConfig<EmailConfig>().To),
//                    Subject = new ElsaExpression("Literal", action.GetConfig<EmailConfig>().Subject),
//                    Body = new ElsaExpression("Literal", action.GetConfig<EmailConfig>().Body)
//                },
//                "HttpCall" => new ElsaHttpEndpointActivity
//                {
//                    Id = action.Id,
//                    NodeId = $"Activity:{action.Id}",
//                    Path = new ElsaExpression("Literal", action.GetConfig<HttpConfig>().Url),
//                    SupportedMethods = new[] { action.GetConfig<HttpConfig>().Method }
//                },
//                "SetVariable" => new ElsaSetVariableActivity
//                {
//                    Id = action.Id,
//                    NodeId = $"Activity:{action.Id}",
//                    VariableName = action.GetConfig<VariableConfig>().Name,
//                    Value = new ElsaExpression("Literal", action.GetConfig<VariableConfig>().Value)
//                },
//                "WriteLine" => new ElsaWriteLineActivity
//                {
//                    Id = action.Id,
//                    NodeId = $"Activity:{action.Id}",
//                    Text = new ElsaExpression("Literal", action.GetConfig<WriteLineConfig>().Text)
//                },
//                _ => CreateNoOpActivity(null, action.Id)
//            };
//        }

//        private ElsaActivity CreateDelayActivity(WorkflowStep step)
//        {
//            var timeoutMinutes = step.TimeoutMinutes ?? 5;

//            return new ElsaDelayActivity
//            {
//                Id = step.Id,
//                NodeId = $"Activity:{step.Id}",
//                TimeSpan = new ElsaExpression("Literal", $"00:{timeoutMinutes:D2}:00")
//            };
//        }

//        private ElsaActivity CreateParallelActivity(WorkflowStep step)
//        {
//            return new ElsaParallelForkActivity
//            {
//                Id = step.Id,
//                NodeId = $"Activity:{step.Id}",
//                Branches = step.Tasks.Select(t => t.TaskName).ToList()
//            };
//        }

//        private ElsaActivity CreateDecisionActivity(WorkflowStep step)
//        {
//            return new ElsaIfElseActivity
//            {
//                Id = step.Id,
//                NodeId = $"Activity:{step.Id}",
//                Condition = new ElsaExpression("JavaScript", step.Tasks.FirstOrDefault()?.RuleSetId ?? "true")
//            };
//        }

//        private ElsaActivity CreateNoOpActivity(WorkflowStep? step = null, string? customId = null)
//        {
//            var id = step?.Id ?? customId ?? Guid.NewGuid().ToString();

//            return new ElsaNoOpActivity
//            {
//                Id = id,
//                NodeId = $"Activity:{id}"
//            };
//        }

//        private List<ElsaConnection> CreateSequentialConnections(List<ElsaActivity> activities)
//        {
//            var connections = new List<ElsaConnection>();

//            for (int i = 0; i < activities.Count - 1; i++)
//            {
//                connections.Add(new ElsaConnection
//                {
//                    Source = new ElsaConnectionEndpoint { Activity = activities[i].Id, Port = "Done" },
//                    Target = new ElsaConnectionEndpoint { Activity = activities[i + 1].Id, Port = "In" }
//                });
//            }

//            return connections;
//        }

//        private List<ElsaVariable> BuildVariables(Workflowdefinition definition)
//        {
//            return new List<ElsaVariable>
//            {
//                new()
//                {
//                    Id = Guid.NewGuid().ToString(),
//                    Name = "TenantId",
//                    TypeName = "String",
//                    Value = new ElsaExpression("Literal", definition.TenantId)
//                },
//                new()
//                {
//                    Id = Guid.NewGuid().ToString(),
//                    Name = "DomainId",
//                    TypeName = "String",
//                    Value = new ElsaExpression("Literal", definition.DomainId)
//                }
//            };
//        }
//    }

//    #region Elsa 3.x DTOs (Matching expected JSON structure)

//    public class ElsaWorkflowDefinition
//    {
//        public string DefinitionId { get; set; } = default!;
//        public string Name { get; set; } = default!;
//        public string? Description { get; set; }
//        public int Version { get; set; }
//        public ElsaFlowchart Root { get; set; } = default!;
//        public List<ElsaVariable> Variables { get; set; } = new();
//        public List<ElsaInputDefinition> Inputs { get; set; } = new();
//        public List<ElsaOutputDefinition> Outputs { get; set; } = new();
//    }

//    public class ElsaFlowchart
//    {
//        public string Id { get; set; } = default!;
//        public string Type => "Elsa.Flowchart";
//        public List<ElsaActivity> Activities { get; set; } = new();
//        public List<ElsaConnection> Connections { get; set; } = new();
//    }

//    public class ElsaActivity
//    {
//        public string Id { get; set; } = default!;
//        public string NodeId { get; set; } = default!;
//        public string Type { get; set; } = default!;
//    }

//    public class ElsaSendEmailActivity : ElsaActivity
//    {
//        public ElsaExpression To { get; set; } = default!;
//        public ElsaExpression Subject { get; set; } = default!;
//        public ElsaExpression Body { get; set; } = default!;

//        public ElsaSendEmailActivity() => Type = "Elsa.SendEmail";
//    }

//    public class ElsaHttpEndpointActivity : ElsaActivity
//    {
//        public ElsaExpression Path { get; set; } = default!;
//        public string[] SupportedMethods { get; set; } = default!;

//        public ElsaHttpEndpointActivity() => Type = "Elsa.HttpEndpoint";
//    }

//    public class ElsaSetVariableActivity : ElsaActivity
//    {
//        public string VariableName { get; set; } = default!;
//        public ElsaExpression Value { get; set; } = default!;

//        public ElsaSetVariableActivity() => Type = "Elsa.SetVariable";
//    }

//    public class ElsaWriteLineActivity : ElsaActivity
//    {
//        public ElsaExpression Text { get; set; } = default!;

//        public ElsaWriteLineActivity() => Type = "Elsa.WriteLine";
//    }

//    public class ElsaDelayActivity : ElsaActivity
//    {
//        public ElsaExpression TimeSpan { get; set; } = default!;

//        public ElsaDelayActivity() => Type = "Elsa.Delay";
//    }

//    public class ElsaParallelForkActivity : ElsaActivity
//    {
//        public List<string> Branches { get; set; } = new();

//        public ElsaParallelForkActivity() => Type = "Elsa.ParallelFork";
//    }

//    public class ElsaIfElseActivity : ElsaActivity
//    {
//        public ElsaExpression Condition { get; set; } = default!;

//        public ElsaIfElseActivity() => Type = "Elsa.IfElse";
//    }

//    public class ElsaSequenceActivity : ElsaActivity
//    {
//        public List<ElsaActivity> Activities { get; set; } = new();
//        public List<ElsaConnection> Connections { get; set; } = new();

//        public ElsaSequenceActivity() => Type = "Elsa.Sequence";
//    }

//    public class ElsaNoOpActivity : ElsaActivity
//    {
//        public ElsaNoOpActivity() => Type = "Elsa.NoOp";
//    }

//    public class ElsaConnection
//    {
//        public ElsaConnectionEndpoint Source { get; set; } = default!;
//        public ElsaConnectionEndpoint Target { get; set; } = default!;
//    }

//    public class ElsaConnectionEndpoint
//    {
//        public string Activity { get; set; } = default!;
//        public string Port { get; set; } = default!;
//    }

//    public class ElsaVariable
//    {
//        public string Id { get; set; } = default!;
//        public string Name { get; set; } = default!;
//        public string TypeName { get; set; } = default!;
//        public ElsaExpression? Value { get; set; }
//    }

//    public class ElsaExpression
//    {
//        public string Type { get; set; }
//        public object? Value { get; set; }

//        public ElsaExpression(string type, object? value = null)
//        {
//            Type = type;
//            Value = value;
//        }
//    }

//    public class ElsaInputDefinition
//    {
//        public string Name { get; set; } = default!;
//        public string Type { get; set; } = default!;
//    }

//    public class ElsaOutputDefinition
//    {
//        public string Name { get; set; } = default!;
//        public string Type { get; set; } = default!;
//    }

//    #endregion
//}



// EnterpriseWorkflow.Elsa/Mapping/EnterpriseToElsaMapper.cs