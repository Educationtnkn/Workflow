using EnterpriseWorkflow.Utilities.Observability;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseWorkflow.Utilities.DbConnectors
{
    // ─────────────────────────────────────────────────────────────────────────────
    // TVP DEFINITION
    // Caller builds a DataTable and wraps it with the SQL UDT type name
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Represents a Table-Valued Parameter to be passed to a stored procedure.
    /// </summary>
    public sealed class TvpParameter
    {
        /// <summary>
        /// The SQL Server UDT name, e.g. "idp_proc.t_File_Doc_Reference_Save"
        /// </summary>
        public required string TypeName { get; init; }

        /// <summary>
        /// DataTable whose columns match the SQL UDT definition (column order matters).
        /// </summary>
        public required DataTable Data { get; init; }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // REQUEST MODEL
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generic request for executing any stored procedure.
    /// </summary>
    public sealed class SpExecutionRequest
    {
        /// <summary>Stored procedure name without schema, e.g. "sp_Save_File_Docs"</summary>
        public required string ProcedureName { get; init; }

        /// <summary>Schema name, e.g. "idp_proc". Defaults to "dbo" if not provided.</summary>

        /// <summary>Command timeout in seconds. Defaults to 60.</summary>
        public int CommandTimeout { get; init; } = 60;

        // ── Scalar input parameters ───────────────────────────────────────────────

        /// <summary>
        /// Scalar input parameters: name → value.
        /// Use null value to pass SQL NULL. Type inference is automatic.
        /// For explicit type control use InputParameterTypes.
        /// </summary>
        public Dictionary<string, object?>? InputParameters { get; init; }

        /// <summary>
        /// Optional explicit SqlDbType per input parameter.
        /// Required for ambiguous types (e.g. Char vs VarChar, SmallInt vs Int).
        /// </summary>
        public Dictionary<string, SqlDbType>? InputParameterTypes { get; init; }

        /// <summary>
        /// Optional size per input parameter.
        /// Required for VARCHAR/NVARCHAR/VARBINARY/CHAR typed parameters.
        /// Use -1 for MAX.
        /// </summary>
        public Dictionary<string, int>? InputParameterSizes { get; init; }

        // ── Table-Valued Parameters ───────────────────────────────────────────────

        /// <summary>
        /// TVP parameters: name → TvpParameter (TypeName + DataTable).
        /// e.g. "@p_File_Doc_Reference" → new TvpParameter { TypeName = "idp_proc.t_File_Doc_Reference_Save", Data = dt }
        /// </summary>
        public Dictionary<string, TvpParameter>? TvpParameters { get; init; }

        // ── Output parameters ─────────────────────────────────────────────────────

        /// <summary>
        /// Output parameter definitions: name → SqlDbType.
        /// Values are populated in SpExecutionResult.OutputParameters after execution.
        /// </summary>
        public Dictionary<string, SqlDbType>? OutputParameters { get; init; }

        /// <summary>
        /// Optional size per output parameter.
        /// Required for NVARCHAR/VARCHAR output params. Use -1 for MAX.
        /// </summary>
        public Dictionary<string, int>? OutputParameterSizes { get; init; }

        // ── Logging / tracing ─────────────────────────────────────────────────────

        public string ServiceName { get; init; } = string.Empty;
        public string CorrelationId { get; init; } = string.Empty;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // RESULT MODEL
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Result of stored procedure execution.
    /// </summary>
    public sealed class SpExecutionResult
    {
        public bool Success { get; set; }

        /// <summary>
        /// All result sets returned by the SP (SELECT outputs).
        /// Each result set is a list of rows; each row is column-name → value.
        /// </summary>
        public List<List<Dictionary<string, object?>>> ResultSets { get; set; } = [];

        /// <summary>
        /// Output parameter values after execution: parameter name → value.
        /// </summary>
        public Dictionary<string, object?> OutputParameters { get; set; } = [];

        /// <summary>
        /// RETURN VALUE from the stored procedure (if any).
        /// </summary>
        public int? ReturnValue { get; set; }

        /// <summary>
        /// Total rows across all result sets, or RecordsAffected for non-SELECT procs.
        /// </summary>
        public int RowsAffected { get; set; }

        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }

        // Convenience: first result set
        public List<Dictionary<string, object?>>? FirstResultSet =>
            ResultSets.Count > 0 ? ResultSets[0] : null;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // GENERIC SP CONNECTOR
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Generic stored procedure connector built directly on Microsoft.Data.SqlClient.
    /// Supports scalar inputs, TVPs, output parameters, multiple result sets,
    /// and return values — with no third-party wrapper dependencies.
    /// </summary>
    public sealed class SqlServerConnector2
    {
        private readonly ILoggingService _logger;
        private readonly string _connectionString;
        private readonly string _schemaName;

        public SqlServerConnector2(IConfiguration configuration, ILoggingService logger)
        {
            _logger = logger;

            _connectionString = configuration["DB_Provider:RelationalDatabase:SqlServer:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Connection string 'SqlServer' is not configured in appsettings.");

            _schemaName = configuration["DB_Provider:RelationalDatabase:SqlServer:SchemaName"] ?? "dbo";
        }

        // ── PRIMARY EXECUTION METHOD ──────────────────────────────────────────────

        public async Task<SpExecutionResult> ExecuteAsync(
            SpExecutionRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var correlationId = request.CorrelationId ?? Guid.NewGuid().ToString();
            var serviceName = request.ServiceName ?? "SqlServerConnector";
            var procedureName = $"[{_schemaName}].[{request.ProcedureName}]";

            var filterIds = new Dictionary<string, object>
            {
                ["Procedure"] = procedureName
            };

            _logger.LogInformation(serviceName, $"Stored procedure execution started Proc Name: {procedureName}", correlationId, new Dictionary<string, object> { ["Procedure"] = procedureName });

            var result = new SpExecutionResult();

            var fullyQualifiedName = $"[{_schemaName}].[{request.ProcedureName}]";

            try
            {
                _logger.LogDebug(serviceName, "Opening SQL connection", correlationId, new Dictionary<string, object> { ["Procedure"] = procedureName });

                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync(cancellationToken);

                await using var command = new SqlCommand(fullyQualifiedName, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = request.CommandTimeout
                };
                _logger.LogDebug(serviceName, "Preparing SQL parameters", correlationId, filterIds, new Dictionary<string, object> { ["InputParamCount"] = request.InputParameters?.Count ?? 0, ["TvpParamCount"] = request.TvpParameters?.Count ?? 0, ["OutputParamCount"] = request.OutputParameters?.Count ?? 0, ["Timeout"] = request.CommandTimeout });

                // 1. Scalar input parameters
                AddScalarInputParameters(command, request);

                // 2. TVP parameters
                AddTvpParameters(command, request);

                // 3. Output parameters — collect SqlParameter refs for value retrieval
                var outputRefs = AddOutputParameters(command, request);

                // 4. Return value parameter
                var returnParam = new SqlParameter("@ReturnValue", SqlDbType.Int)
                {
                    Direction = ParameterDirection.ReturnValue
                };
                command.Parameters.Add(returnParam);
                _logger.LogInformation(serviceName, "Executing stored procedure", correlationId, new Dictionary<string, object> { ["Procedure"] = procedureName });

                // 5. Execute and read all result sets
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                result.ResultSets = await ReadAllResultSetsAsync(reader, cancellationToken);

                // Reader must be closed before output param values are accessible
                await reader.CloseAsync();

                // 6. Collect output parameter values
                foreach (var (name, sqlParam) in outputRefs)
                {
                    result.OutputParameters[name] = sqlParam.Value == DBNull.Value
                        ? null
                        : sqlParam.Value;
                }

                // 7. Return value
                if (returnParam.Value != DBNull.Value && returnParam.Value is not null)
                    result.ReturnValue = (int)returnParam.Value;

                // 8. RowsAffected
                result.RowsAffected = result.ResultSets.Count > 0
                    ? result.ResultSets.Sum(rs => rs.Count)
                    : reader.RecordsAffected;
                _logger.LogInformation(serviceName, "Stored procedure executed successfully", correlationId, new Dictionary<string, object> { ["Procedure"] = procedureName, ["ResultSetCount"] = result.ResultSets.Count, ["RowsAffected"] = result.RowsAffected, ["ReturnValue"] = result.ReturnValue });

                result.Success = true;
            }
            catch (SqlException ex)
            {
                _logger.LogError(serviceName, "SQL error during stored procedure execution", $"SQL_{ex.Number}", "SqlException", ex.Message, correlationId, new Dictionary<string, object> { ["Procedure"] = procedureName, ["SqlNumber"] = ex.Number, ["Severity"] = ex.Class }, ex);

                result.Success = false;
                result.ErrorCode = $"SQL_{ex.Number}";
                result.ErrorMessage = $"[SqlException] Number={ex.Number} Severity={ex.Class}: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(serviceName, "Unexpected error during stored procedure execution", "UNEXPECTED_ERROR", "SystemException", ex.Message, correlationId, new Dictionary<string, object> { ["Procedure"] = procedureName }, ex);

                result.Success = false;
                result.ErrorCode = "UNEXPECTED_ERROR";
                result.ErrorMessage = $"[Exception] {ex.GetType().Name}: {ex.Message}";
            }

            return result;
        }

        // ─────────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────────

        // ── 1. SCALAR INPUTS ──────────────────────────────────────────────────────

        private static void AddScalarInputParameters(
            SqlCommand command,
            SpExecutionRequest request)
        {
            if (request.InputParameters is null || request.InputParameters.Count == 0)
                return;

            foreach (var (name, value) in request.InputParameters)
            {
                // Resolve explicit SqlDbType if provided, otherwise let AddWithValue infer
                if (request.InputParameterTypes is not null &&
                    request.InputParameterTypes.TryGetValue(name, out var dbType))
                {
                    var param = new SqlParameter(name, dbType)
                    {
                        Value = value ?? DBNull.Value
                    };

                    // Apply size if specified (required for VARCHAR/NVARCHAR/VARBINARY)
                    if (request.InputParameterSizes is not null &&
                        request.InputParameterSizes.TryGetValue(name, out var size))
                    {
                        param.Size = size; // use -1 for MAX
                    }

                    command.Parameters.Add(param);
                }
                else
                {
                    // Type inference via AddWithValue — fine for unambiguous types
                    var param = command.Parameters.AddWithValue(name, value ?? DBNull.Value);

                    if (request.InputParameterSizes is not null &&
                        request.InputParameterSizes.TryGetValue(name, out var size))
                    {
                        param.Size = size;
                    }
                }
            }
        }

        // ── 2. TVP PARAMETERS ─────────────────────────────────────────────────────

        private static void AddTvpParameters(
            SqlCommand command,
            SpExecutionRequest request)
        {
            if (request.TvpParameters is null || request.TvpParameters.Count == 0)
                return;

            foreach (var (name, tvp) in request.TvpParameters)
            {
                if (string.IsNullOrWhiteSpace(tvp.TypeName))
                    throw new ArgumentException(
                        $"TVP parameter '{name}' has no TypeName set. " +
                        $"Provide the fully qualified SQL UDT name, e.g. 'dbo.MyTableType'.");

                var param = new SqlParameter(name, SqlDbType.Structured)
                {
                    TypeName = tvp.TypeName,    // e.g. "idp_proc.t_File_Doc_Reference_Save"
                    Value = tvp.Data         // DataTable matching the UDT schema
                };

                command.Parameters.Add(param);
            }
        }

        // ── 3. OUTPUT PARAMETERS ──────────────────────────────────────────────────

        private static Dictionary<string, SqlParameter> AddOutputParameters(
            SqlCommand command,
            SpExecutionRequest request)
        {
            var refs = new Dictionary<string, SqlParameter>();

            if (request.OutputParameters is null || request.OutputParameters.Count == 0)
                return refs;

            foreach (var (name, dbType) in request.OutputParameters)
            {
                var param = new SqlParameter(name, dbType)
                {
                    Direction = ParameterDirection.Output
                };

                if (request.OutputParameterSizes is not null &&
                    request.OutputParameterSizes.TryGetValue(name, out var size))
                {
                    param.Size = size; // use -1 for NVARCHAR(MAX) / VARCHAR(MAX)
                }

                command.Parameters.Add(param);
                refs[name] = param; // keep ref so we can read value after reader closes
            }

            return refs;
        }

        // ── 4. READ ALL RESULT SETS ───────────────────────────────────────────────

        private static async Task<List<List<Dictionary<string, object?>>>> ReadAllResultSetsAsync(
            SqlDataReader reader,
            CancellationToken cancellationToken)
        {
            var allSets = new List<List<Dictionary<string, object?>>>();

            do
            {
                if (!reader.HasRows)
                    continue;

                var rows = new List<Dictionary<string, object?>>();

                while (await reader.ReadAsync(cancellationToken))
                {
                    var row = new Dictionary<string, object?>(reader.FieldCount);

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var colValue = reader.GetValue(i);
                        row[reader.GetName(i)] = colValue == DBNull.Value ? null : colValue;
                    }

                    rows.Add(row);
                }

                allSets.Add(rows);
            }
            while (await reader.NextResultAsync(cancellationToken));

            return allSets;
        }
    }
}
