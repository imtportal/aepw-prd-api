using System.Data;
using AEPW.Application.DTOs.Logging;
using AEPW.Application.Interfaces.Repositories.Logging;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using AEPW.Domain.Constants;

namespace AEPW.Infrastructure.Repositories.Logging;

public sealed class ExceptionLogRepository : IExceptionLogRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public ExceptionLogRepository(
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task LogAsync(
        ExceptionLogDto exceptionLog,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(
                cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Logging.InsertExceptionLog,
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar)
        {
            Value = exceptionLog.Message
        });

        command.Parameters.Add(new SqlParameter("@InnerException", SqlDbType.NVarChar)
        {
            Value = (object?)exceptionLog.InnerException ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@StackTrace", SqlDbType.NVarChar)
        {
            Value = (object?)exceptionLog.StackTrace ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@Source", SqlDbType.NVarChar, 500)
        {
            Value = (object?)exceptionLog.Source ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@ClassName", SqlDbType.NVarChar, 500)
        {
            Value = (object?)exceptionLog.ClassName ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@MethodName", SqlDbType.NVarChar, 300)
        {
            Value = (object?)exceptionLog.MethodName ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@FileName", SqlDbType.NVarChar, 500)
        {
            Value = (object?)exceptionLog.FileName ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@LineNumber", SqlDbType.Int)
        {
            Value = (object?)exceptionLog.LineNumber ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@RequestPath", SqlDbType.NVarChar, 500)
        {
            Value = (object?)exceptionLog.RequestPath ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@HttpMethod", SqlDbType.NVarChar, 20)
        {
            Value = (object?)exceptionLog.HttpMethod ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@QueryString", SqlDbType.NVarChar)
        {
            Value = (object?)exceptionLog.QueryString ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@CorrelationId", SqlDbType.UniqueIdentifier)
        {
            Value = Guid.TryParse(exceptionLog.CorrelationId, out var correlationId)
                ? correlationId
                : DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt)
        {
            Value = (object?)exceptionLog.UserId ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@MachineName", SqlDbType.NVarChar, 200)
        {
            Value = (object?)exceptionLog.MachineName ?? DBNull.Value
        });

        command.Parameters.Add(new SqlParameter("@EnvironmentName", SqlDbType.NVarChar, 100)
        {
            Value = (object?)exceptionLog.EnvironmentName ?? DBNull.Value
        });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}