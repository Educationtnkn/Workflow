using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Domain.Configurations
{
    public class EmailConfig
    {
        public string To { get; set; } = string.Empty;
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsBodyHtml { get; set; } = false;
        public Dictionary<string, string>? Headers { get; set; }
    }

    /// <summary>
    /// Configuration for HTTP call action
    /// </summary>
    public class HttpConfig
    {
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = "GET"; // GET, POST, PUT, DELETE, PATCH
        public Dictionary<string, string>? Headers { get; set; }
        public object? Body { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
        public bool FollowRedirects { get; set; } = true;
        public string? ContentType { get; set; } = "application/json";
    }

    /// <summary>
    /// Configuration for WriteLine (logging) action
    /// </summary>
    public class WriteLineConfig
    {
        public string Text { get; set; } = string.Empty;
        public string? LogLevel { get; set; } = "Information"; // Information, Warning, Error, Debug
        public string? Category { get; set; }
    }

    /// <summary>
    /// Configuration for SetVariable action
    /// </summary>
    public class VariableConfig
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Type { get; set; } = "String"; // String, Number, Boolean, Object, Array
    }

    /// <summary>
    /// Configuration for Wait/Timer action
    /// </summary>
    public class WaitConfig
    {
        public int DurationSeconds { get; set; } = 30;
        public string? UntilDateTime { get; set; } // ISO 8601 format
        public string? Expression { get; set; } // JavaScript expression for dynamic wait
    }

    /// <summary>
    /// Configuration for Webhook action
    /// </summary>
    public class WebhookConfig
    {
        public string Url { get; set; } = string.Empty;
        public string Method { get; set; } = "POST";
        public Dictionary<string, string>? Headers { get; set; }
        public object? Payload { get; set; }
        public bool WaitForResponse { get; set; } = false;
    }

    /// <summary>
    /// Configuration for Approval action (human task)
    /// </summary>
    public class ApprovalConfig
    {
        public string ApproverRole { get; set; } = string.Empty;
        public string[]? ApproverEmails { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TimeoutHours { get; set; } = 48;
        public bool AllowRejection { get; set; } = true;
        public string? EscalationEmail { get; set; }
    }

    /// <summary>
    /// Configuration for Database query action
    /// </summary>
    public class DatabaseQueryConfig
    {
        public string ConnectionStringName { get; set; } = string.Empty;
        public string Query { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
        public int CommandTimeoutSeconds { get; set; } = 30;
        public bool IsStoredProcedure { get; set; } = false;
    }

    //public class RunJavaScriptConfig
    //{
    //    public string Script { get; set; } = string.Empty;
    //}
}
