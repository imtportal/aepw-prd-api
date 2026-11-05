using AEPW.Application.DTOs.Authorization;
using AEPW.Application.Interfaces.Repositories.Authorization;
using AEPW.Domain.Constants;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AEPW.Infrastructure.Repositories.Authorization
{
    public sealed class AuthorizationRepository : IAuthorizationUserRepository
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public AuthorizationRepository(ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<LoginContextDto?> GetLoginContextAsync(
            Guid azureAdObjectId,
            CancellationToken cancellationToken = default)
        {
            var loginContext = new LoginContextDto();

            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.Authorization.GetLoginContext,
                connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@AzureAdObjectId", SqlDbType.UniqueIdentifier)
            {
                Value = azureAdObjectId
            });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            // ── Result Set 1: User Details ──
            if (await reader.ReadAsync(cancellationToken))
            {
                loginContext.UserDetails = new UserDetailsDto
                {
                    UserId = reader["UserId"] != DBNull.Value ? Convert.ToInt64(reader["UserId"]) : 0,
                    DisplayName = reader["DisplayName"]?.ToString() ?? string.Empty,
                    Email = reader["Email"]?.ToString() ?? string.Empty
                };
            }
            else
            {
                // No user found
                return null;
            }

            // Page/Module Access ──
            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    loginContext.PageAccess.Add(new PageAccessDto
                    {
                        Key = reader["Key"]?.ToString() ?? string.Empty,
                        Value = reader["Value"] != DBNull.Value && Convert.ToBoolean(reader["Value"])
                    });
                }
            }

            return loginContext;
        }

        public async Task<PageAccessDto> GetAppUserPageAccessAsync(
            string pageKey,
            long userId,
            CancellationToken cancellationToken = default)
        {

            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.Authorization.GetAppUserPageAccess,
                connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@PageKey", SqlDbType.VarChar, 50) { Value = pageKey });
            command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) { Value = userId });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);
            // ── Result Set 1: Page Access ──
            if (!await reader.ReadAsync(cancellationToken))
            {
                return null;
            }

            return new PageAccessDto
            {
                Key = reader["Key"]?.ToString() ?? string.Empty,
                Value = reader["Value"] != DBNull.Value &&
                        Convert.ToBoolean(reader["Value"])
            };
        }
        public async Task<List<ActionAccessDto>> GetAppUserActionAccessAsync(
    string pageKey,
    long userId,
    CancellationToken cancellationToken = default)
        {
            var actionAccess = new List<ActionAccessDto>();

            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.Authorization.GetAppUserActionAccess,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter("@PageKey", SqlDbType.VarChar, 50)
                {
                    Value = pageKey
                });

            command.Parameters.Add(
                new SqlParameter("@UserId", SqlDbType.BigInt)
                {
                    Value = userId
                });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            // ── Result Set 1: Action Access ──
            while (await reader.ReadAsync(cancellationToken))
            {
                actionAccess.Add(new ActionAccessDto
                {
                    Key = reader["Key"]?.ToString() ?? string.Empty,
                    Value = reader["Value"] != DBNull.Value &&
                            Convert.ToBoolean(reader["Value"])
                });
            }

            return actionAccess;
        }
    }
}
