using Microsoft.Data.SqlClient;

namespace AEPW.Infrastructure.Data;

/// <summary>
/// Contract for creating SqlConnection instances.
/// Centralises connection string management and allows easy test substitution.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Creates and opens a new SqlConnection.
    /// Callers are responsible for disposing the connection (use <c>await using</c>).
    /// </summary>
    Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken ct = default);
}
