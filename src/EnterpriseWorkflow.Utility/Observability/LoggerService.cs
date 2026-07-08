using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using ND.Observability.Framework.Core.Application.Handlers.Interface.Logging;
using EnterpriseWorkflow.Utilities.CommonUtility;


namespace EnterpriseWorkflow.Utilities.Observability
{
    public class LoggingService : ILoggingService
    {
        private readonly INDLoggerService _logger;
        private readonly CorrelationIdHelper _correlationIdHelper;

        public LoggingService(
            INDLoggerService logger,
            CorrelationIdHelper correlationIdHelper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _correlationIdHelper = correlationIdHelper ?? throw new ArgumentNullException(nameof(correlationIdHelper));
        }

        private string ResolveCorrelationId(string? correlationId)
        {
            return _correlationIdHelper.GetCorrelationId(correlationId);
        }

        // ===========================
        // Log Information
        // ===========================
        public void LogInformation(
            string serviceName,
            string message,
            string? correlationId = null,
            Dictionary<string, object>? filterIds = null, [CallerMemberName] string method = "")
        {
            _logger.LogInformation(
                serviceName: serviceName,
                message: message,
                method: method,
                correlationId: ResolveCorrelationId(correlationId),
                data: null,
                customUniqueIds: filterIds,
                exceptionCategory: null);
        }

        // ===========================
        // Log Debug
        // ===========================
        public void LogDebug(
            string serviceName,
            string message,

            string? correlationId = null,
            Dictionary<string, object>? filterIds = null,
            Dictionary<string, object>? data = null, [CallerMemberName] string method = "")
        {
            _logger.LogDebug(
                serviceName: serviceName,
                message: message,
                method: method,
                correlationId: ResolveCorrelationId(correlationId),
                data: data,
                customUniqueIds: filterIds,
                exceptionCategory: null);
        }

        // ===========================
        // Log Warning
        // ===========================
        public void LogWarning(
            string serviceName,
            string message,
            string? errorCode = null,
            string? errorCategory = null,
            string? errorDetails = null,
            string? correlationId = null,
            Dictionary<string, object>? filterIds = null,
            Exception? exception = null, [CallerMemberName] string method = "")
        {
            _logger.LogWarning(
                serviceName: serviceName,
                message: message,
                method: method,
                correlationId: ResolveCorrelationId(correlationId),
                data: null,
                customUniqueIds: filterIds,
                exceptionCategory: errorCategory,
                errorCode: errorCode,
                details: errorDetails);
        }

        // ===========================
        // Log Error
        // ===========================
        public void LogError(
            string serviceName,
            string message,
            string? errorCode = null,
            string? errorCategory = null,
            string? errorDetails = null,

            string? correlationId = null,
            Dictionary<string, object>? filterIds = null,
            Exception? exception = null, [CallerMemberName] string method = "")
        {
            _logger.LogError(
                serviceName: serviceName,
                message: message,
                method: method,
                correlationId: ResolveCorrelationId(correlationId),
                data: null,
                customUniqueIds: filterIds,
                exceptionCategory: errorCategory,
                errorCode: errorCode,
                details: errorDetails,
                stackTrace: exception?.StackTrace,
                exception: exception);
        }

        // ===========================
        // Log Fatal
        // ===========================
        public void LogFatal(
            string serviceName,
            string message,
            string? errorCode = null,
            string? errorCategory = null,
            string? errorDetails = null,
            string? correlationId = null,
            Dictionary<string, object>? filterIds = null,
            Exception? exception = null, [CallerMemberName] string method = "")
        {
            _logger.LogFatal(
                serviceName: serviceName,
                message: message,
                method: method,
                correlationId: ResolveCorrelationId(correlationId),
                data: null,
                customUniqueIds: filterIds,
                exceptionCategory: errorCategory,
                errorCode: errorCode,
                details: errorDetails,
                stackTrace: exception?.StackTrace,
                exception: exception);
        }
    }
}