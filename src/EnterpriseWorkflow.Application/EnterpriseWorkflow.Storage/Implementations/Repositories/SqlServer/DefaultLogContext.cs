namespace EnterpriseWorkflow.Storage.Implementations.Repositories.SqlServer
{
    internal class DefaultLogContext
    {
        public object MethodName { get; set; }
        public object ServiceName { get; set; }
        public string? CorrelationId { get; set; }
        public object LogFilters { get; set; }
    }
}