using AEPW.Infrastructure.Configurations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace AEPW.Infrastructure.Data;

/// Default SQL connection factory backed by Microsoft.Data.SqlClient.
/// Connection pooling is handled by the ADO.NET pool; we simply open/return connections.

public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly DatabaseSettings _settings;

    public SqlConnectionFactory(IOptions<DatabaseSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<SqlConnection> CreateOpenConnectionAsync(CancellationToken ct = default)
    {
        var connection = new SqlConnection(_settings.ConnectionString);
        
       await connection.OpenAsync(ct);
        return connection;
    }
}
