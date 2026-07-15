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
        private const string InstanceNumberKey = "InstanceNumber";

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

        //public static long GetEnterpriseInstanceId(this WorkflowExecutionContext ctx)
        //{
        //    // WorkflowInput is populated on ActivityExecutionContext even when
        //    // Variables/Properties are empty (e.g. inside a nested ExecuteWorkflow activity)
        //    return ctx.Properties.TryGetValue("EnterpriseInstanceId", out var value)
        //   ? Convert.ToInt64(value)
        //   : 0;
        //}

       // public static long GetEnterpriseExecutionId(this WorkflowExecutionContext ctx)
       // {
       //     return ctx.Properties.TryGetValue("EnterpriseExecutionId", out var value)
       //? Convert.ToInt64(value)
       //: 0;
       // }



        public static string? GetInstanceNumber(this ActivityExecutionContext ctx)
        {
            if (ctx.WorkflowInput != null &&
                ctx.WorkflowInput.TryGetValue(InstanceNumberKey, out var val) && val != null)
            {
                return val.ToString();
            }

            return ctx.WorkflowExecutionContext.GetInstanceNumber();
        }

        //public static string? GetInstanceNumber(this WorkflowExecutionContext ctx)
        //{
        //    if (ctx.Input != null &&
        //      ctx.Input.TryGetValue(InstanceNumberKey, out var val) && val != null)
        //    {
        //        return val.ToString();
        //    }

        //    return "0";
        //}


        public static string? GetInstanceNumber(this WorkflowExecutionContext ctx)
        {
            if (ctx.Input != null && ctx.Input.TryGetValue(InstanceNumberKey, out var val) && val != null)
                return val.ToString();
            return "0";
        }

        // Reads ctx.Properties — NOT reliably populated at the workflow level!
        //public static long GetEnterpriseInstanceId(this WorkflowExecutionContext ctx)
        //{
        //    return ctx.Properties.TryGetValue("EnterpriseInstanceId", out var value)
        //        ? Convert.ToInt64(value) : 0;
        //}

        public static long GetEnterpriseInstanceId(this WorkflowExecutionContext ctx)
        {
            if (ctx.Input != null &&
                ctx.Input.TryGetValue(EnterpriseInstanceIdKey, out var inputVal) && inputVal != null)
            {
                return Convert.ToInt64(inputVal);
            }

            return ctx.Properties.TryGetValue(EnterpriseInstanceIdKey, out var propVal)
                ? Convert.ToInt64(propVal)
                : 0;
        }

        public static long GetEnterpriseExecutionId(this WorkflowExecutionContext ctx)
        {
            if (ctx.Input != null &&
                ctx.Input.TryGetValue(EnterpriseExecutionIdKey, out var inputVal) && inputVal != null)
            {
                return Convert.ToInt64(inputVal);
            }

            return ctx.Properties.TryGetValue(EnterpriseExecutionIdKey, out var propVal)
                ? Convert.ToInt64(propVal)
                : 0;
        }
    }
}