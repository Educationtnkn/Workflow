using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AppFW.Utilities.Observability
{
    public interface ILoggingService
    {
        void LogInformation(
            string serviceName,
            string message,
            string? correlationId = null,
            Dictionary<string, object>? filterIds = null, [CallerMemberName] string method = "");

        void LogDebug(
            string serviceName,
            string message,
            string? correlationId = null,
            Dictionary<string, object>? filterIds = null,
            Dictionary<string, object>? data = null, [CallerMemberName] string method = "");

        void LogWarning(
            string serviceName,
            string message,
            string? errorCode = null,
            string? errorCategory = null,
            string? errorDetails = null,
            string? correlationId = null,
            Dictionary<string, object>? filterIds = null,
            Exception? exception = null, [CallerMemberName] string method = "");

        void LogError(
            string serviceName,
            string message,
            string? errorCode = null,
            string? errorCategory = null,
            string? errorDetails = null,
            string? correlationId = null,
            Dictionary<string, object>? filterIds = null,
            Exception? exception = null, [CallerMemberName] string method = "");

        void LogFatal(
            string serviceName,
            string message,
            string? errorCode = null,
            string? errorCategory = null,
            string? errorDetails = null,
            string? correlationId = null,
            Dictionary<string, object>? filterIds = null,
            Exception? exception = null, [CallerMemberName] string method = "");
    }

}
