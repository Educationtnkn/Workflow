using Elsa.Workflows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EnterpriseWorkflow.Elsa.Extensions
{
    public static class ElsaContextExtensions
    {
        private const string EnterpriseInstanceIdKey = "EnterpriseInstanceId";
        private const string EnterpriseExecutionIdKey = "EnterpriseExecutionId";

        // Added to resolve CS1061: provide a way to read variables from WorkflowExecutionContext.
        // Uses reflection to avoid depending on a specific Variable shape; tries 'Name'/'Value' properties.
        public static object? GetVariable(this WorkflowExecutionContext ctx, string name)
        {
            if (ctx == null) throw new ArgumentNullException(nameof(ctx));
            if (string.IsNullOrEmpty(name)) return null;

            var variable = ctx.Variables?.FirstOrDefault(v =>
            {
                if (v == null) return false;
                var nameProp = v.GetType().GetProperty("Name");
                if (nameProp == null) return false;
                var nameVal = nameProp.GetValue(v) as string;
                return string.Equals(nameVal, name, StringComparison.OrdinalIgnoreCase);
            });

            if (variable == null) return null;

            var valueProp = variable.GetType().GetProperty("Value");
            if (valueProp != null) return valueProp.GetValue(variable);

            return null;
        }

        public static long GetEnterpriseInstanceId(this ActivityExecutionContext ctx)
        {
            // WorkflowInput is populated on ActivityExecutionContext even when
            // Variables/Properties are empty (e.g. inside a nested ExecuteWorkflow activity)
            if (ctx.WorkflowInput != null &&
                ctx.WorkflowInput.TryGetValue(EnterpriseInstanceIdKey, out var val) && val != null)
            {
                return Convert.ToInt64(val);
            }

            return ctx.WorkflowExecutionContext.GetEnterpriseInstanceId();
        }

        public static long GetEnterpriseExecutionId(this ActivityExecutionContext ctx)
        {
            if (ctx.WorkflowInput != null &&
                ctx.WorkflowInput.TryGetValue(EnterpriseExecutionIdKey, out var val) && val != null)
            {
                return Convert.ToInt64(val);
            }

            return ctx.WorkflowExecutionContext.GetEnterpriseExecutionId();
        }

        public static long GetEnterpriseInstanceId(this WorkflowExecutionContext ctx)
        {
            // Try variable first, then workflow-execution context properties, then workflow definition properties
            object? value = ctx.GetVariable(EnterpriseInstanceIdKey);

            if (value == null && ctx.Properties != null)
            {
                if (ctx.Properties.TryGetValue(EnterpriseInstanceIdKey, out var propVal))
                    value = propVal;
            }

            // Workflow does not expose a 'Properties' member; use CustomProperties (or Metadata) instead.
            if (value == null && ctx.Workflow?.CustomProperties != null)
            {
                if (ctx.Workflow.CustomProperties.TryGetValue(EnterpriseInstanceIdKey, out var wfPropVal))
                    value = wfPropVal;
            }

            return Convert.ToInt64(value ?? 0L);
        }

        public static long GetEnterpriseExecutionId(this WorkflowExecutionContext ctx)
        {
            object? value = ctx.GetVariable(EnterpriseExecutionIdKey);

            if (value == null && ctx.Properties != null)
            {
                if (ctx.Properties.TryGetValue(EnterpriseExecutionIdKey, out var propVal))
                    value = propVal;
            }

            // Workflow does not expose a 'Properties' member; use CustomProperties (or Metadata) instead.
            if (value == null && ctx.Workflow?.CustomProperties != null)
            {
                if (ctx.Workflow.CustomProperties.TryGetValue(EnterpriseExecutionIdKey, out var wfPropVal))
                    value = wfPropVal;
            }

            return Convert.ToInt64(value ?? 0L);
        }
    }
}