using DB_Provider.Core.Domains;
using DB_Provider.Domain;
using DB_Provider.Interface;
using EnterpriseWorkflow.Utilities.Exceptions;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Utilities.DbConnectors
{
    public class MSSQLConnector
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IMssqlExecutor _executor;
        private readonly IConfiguration _configuration;

        public MSSQLConnector(
            IDbConnectionFactory connectionFactory,
            IMssqlExecutor executor,
            IConfiguration configuration)
        {
            _connectionFactory = connectionFactory;
            _executor = executor;
            _configuration = configuration;
        }

        public async Task<MssqlExecutionResult> ExecuteStoredProcedureAsync(
            string procedureName,
            Dictionary<string, object> inputParameters,
            Dictionary<string, SqlDbType>? inputParameterTypes = null,
            Dictionary<string, SqlDbType>? outputParameters = null,
            Dictionary<string, int>? outputParameterSizes = null,
            string serviceName = "",
            string correlationId = "", string? customSchemaName = null)
        {
            const string methodName = nameof(ExecuteStoredProcedureAsync);

            // 1. Validate configuration
            var schemaName = _configuration["DbProvider:RelationalDatabase:SqlServer:SchemaName"];
            if (string.IsNullOrWhiteSpace(schemaName))
            {
                throw new InvalidOperationException("MSSQL SchemaName not configured");
            }

            var connector = _connectionFactory.Create(DbProviderType.MSSQL);
            await using var connection = await connector.GetSqlServerConnectionAsync();

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                // 2. Build execution request
                var request = new MssqlExecutionRequest
                {
                    OperationName = procedureName,
                    SchemaName = customSchemaName ?? schemaName,

                    InputParameters = inputParameters,
                    InputParameterTypes = inputParameterTypes,

                    OutputParameters = outputParameters,
                    OutputParameterSizes = outputParameterSizes,

                    LogInfo = new MssqlExecutionRequest.LoggingDetails
                    {
                        ServiceName = serviceName,
                        CorrelationId = correlationId
                    }
                };

                // 3. Execute stored procedure
                var result = _executor.ExecuteStoredProcedure(connection, request);

                // 4. Handle DB-level failure
                if (!result.Success &&
                    (result.OutputParameters == null || result.OutputParameters.Count == 0))
                {
                    throw new DatabaseException("DB_EXCEPTION", (result.ErrorMessage ?? result.Message) ?? string.Empty, methodName);
                }

                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                // 5. Defensive close
                if (connection.State != ConnectionState.Closed)
                {
                    await connection.CloseAsync();
                }
            }
        }
    }
}
