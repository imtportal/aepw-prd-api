using System.Data;
using AEPW.Infrastructure.Configurations;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AEPW.Infrastructure.Helpers;

/// <summary>
/// Base class for all repositories.
/// Provides factory helpers for creating stored-procedure commands with
/// a consistent timeout and command type, reducing repetitive boilerplate.
/// </summary>
public abstract class BaseRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly int                   _commandTimeout;

    protected BaseRepository(
        ISqlConnectionFactory       connectionFactory,
        IOptions<DatabaseSettings>  settings)
    {
        _connectionFactory = connectionFactory;
        _commandTimeout    = settings.Value.CommandTimeoutSeconds;
    }

    /// <summary>
    /// Opens a new connection and creates a <see cref="SqlCommand"/> configured for
    /// stored-procedure execution. Both the connection and command are returned so
    /// callers can dispose them with <c>await using</c>.
    /// </summary>
    protected async Task<(SqlConnection Connection, SqlCommand Command)> CreateSpCommandAsync(
        string            storedProcedure,
        CancellationToken ct = default)
    {
        var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        var command    = new SqlCommand(storedProcedure, connection)
        {
            CommandType    = CommandType.StoredProcedure,
            CommandTimeout = _commandTimeout
        };
        return (connection, command);
    }
}
