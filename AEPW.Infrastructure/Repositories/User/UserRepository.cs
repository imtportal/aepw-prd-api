using AEPW.Application.DTOs.Common;
using AEPW.Application.DTOs.Lookup;
using AEPW.Application.Exceptions;
using AEPW.Application.Interfaces.Repositories.User;
using AEPW.Domain.Constants;
using AEPW.Application.DTOs.User;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AEPW.Infrastructure.Repositories.User
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _sqlConnectionFactory;

        public UserRepository(
          ISqlConnectionFactory sqlConnectionFactory)
        {
            _sqlConnectionFactory = sqlConnectionFactory;
        }

        public async Task<UserDetailDto?> GetUserByIdAsync(
            long userId,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.User.GetById,
                connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.BigInt) { Value = userId });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            if (!await reader.ReadAsync(cancellationToken))
                return null;

            return new UserDetailDto
            {
                UserId = reader["UserId"].ToString() ?? string.Empty,
                AzureAdObjectId = reader["AzureAdObjectId"].ToString() ?? string.Empty,
                Upn = reader["Upn"].ToString() ?? string.Empty,
                DisplayName = reader["DisplayName"].ToString() ?? string.Empty,
                Email = reader["Email"].ToString() ?? string.Empty,
                FirstName = reader["FirstName"].ToString() ?? string.Empty,
                LastName = reader["LastName"].ToString() ?? string.Empty,
                Phone = reader["Phone"].ToString() ?? string.Empty,
                JobTitle = reader["JobTitle"].ToString() ?? string.Empty,
                TimeZoneId = reader["TimeZoneId"] == DBNull.Value
                    ? 0
                    : Convert.ToInt64(reader["TimeZoneId"]),

                SystemRoleId = reader["SystemRoleId"].ToString() ?? string.Empty,
                RoleName = reader["RoleName"].ToString() ?? string.Empty,
                IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                LastLoginUtc = reader["LastLoginUtc"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginUtc"]) : null
            };
        }

        public async Task<IReadOnlyList<UserDto>> GetUserByEmail(string email, CancellationToken cancellationToken = default)
        {
            List<UserDto> userDetails = new List<UserDto>();
            await using var connection =
                await _sqlConnectionFactory
                    .CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.User.GetByEmail,
                connection);

            command.CommandType =
                System.Data.CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Email", email);

            DataSet ds = new DataSet();
            await Task.Run(() =>
            {
                using var adapter = new SqlDataAdapter(command);
                adapter.Fill(ds);

                if (ds.Tables != null && ds.Tables.Count > 0)
                {
                    DataTable dt = ds.Tables[0];

                    DataRowCollection rows = dt.Rows;

                    foreach (DataRow row in rows)
                    {
                        userDetails.Add(
                            new UserDto
                            {
                                UserId = row.Field<Int64>("UserId").ToString(),
                                AzureAdObjectId = row.Field<Guid>("AzureAdObjectId").ToString(),
                                Upn = row.Field<string>("Upn"),
                                Email = row.Field<string>("Email"),
                                DisplayName = row.Field<string>("DisplayName"),
                                FirstName = row.Field<string>("FirstName"),
                                LastName = row.Field<string>("LastName"),
                                Phone = row.Field<string>("Phone"),
                                JobTitle = row.Field<string>("JobTitle"),
                                IsActive = row.Field<bool>("IsActive") ? 1 : 0,
                                LastLoginUtc = row.Field<DateTime?>("LastLoginUtc"),//CreatedBy = row.Field<int>("CreatedBy"),CreatedOnUTC = row.Field<DateTime>("CreatedOnUTC"),
                                UpdatedBy = row.Field<long?>("UpdatedBy"),
                                UpdatedOnUTC = row.Field<DateTime?>("UpdatedOnUTC"),

                            }
                        );
                    }
                }
            });

            return userDetails;
        }

        public async Task<PaginatedResult<UserListDto>> GetAllUsersAsync(
                    string? searchText = null,
                    long? systemRoleId = null,
                    bool? isActive = null,
                    int pageNumber = 1,
                    int pageSize = 10,
                    CancellationToken cancellationToken = default)
        {
            var users = new List<UserListDto>();
            int totalCount = 0;

            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.User.GetAll,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@SearchText",
                string.IsNullOrWhiteSpace(searchText)
                    ? DBNull.Value
                    : searchText);

            command.Parameters.AddWithValue(
                "@SystemRoleId",
                systemRoleId.HasValue
                    ? systemRoleId.Value
                    : DBNull.Value);
            command.Parameters.AddWithValue(
               "@IsActive",
                 isActive.HasValue
        ? isActive.Value
        : DBNull.Value);
            command.Parameters.AddWithValue("@PageNumber", pageNumber);
            command.Parameters.AddWithValue("@PageSize", pageSize);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                // Read TotalCount from the first row
                if (totalCount == 0 && reader["TotalCount"] != DBNull.Value)
                {
                    totalCount = Convert.ToInt32(reader["TotalCount"]);
                }

                users.Add(new UserListDto
                {
                    UserId = reader["UserId"]?.ToString() ?? string.Empty,
                    DisplayName = reader["DisplayName"]?.ToString() ?? string.Empty,
                    Upn = reader["Upn"]?.ToString() ?? string.Empty,
                    Email = reader["Email"]?.ToString() ?? string.Empty,
                    AzureAdObjectId = reader["AzureAdObjectId"]?.ToString() ?? string.Empty,
                    SystemRoleId = reader["SystemRoleId"]?.ToString() ?? string.Empty,
                    RoleName = reader["RoleName"]?.ToString() ?? string.Empty,
                    IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                    LastLoginUtc = reader["LastLoginUtc"] != DBNull.Value ? Convert.ToDateTime(reader["LastLoginUtc"]) : null
                });
            }

            return new PaginatedResult<UserListDto>
            {
                Items = users,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<CreateUserResponseDto> CreateUserAsync(
            CreateUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.User.Create,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@AzureAdObjectId", SqlDbType.UniqueIdentifier)
            { Value = request.AzureAdObjectId });
            command.Parameters.Add(new SqlParameter("@Upn", SqlDbType.NVarChar, 256)
            { Value = request.Upn });
            command.Parameters.Add(new SqlParameter("@DisplayName", SqlDbType.NVarChar, 256)
            { Value = request.DisplayName });
            command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 256)
            { Value = (object?)request.Email ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@FirstName", SqlDbType.NVarChar, 100)
            { Value = (object?)request.FirstName ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@LastName", SqlDbType.NVarChar, 100)
            { Value = (object?)request.LastName ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@Phone", SqlDbType.NVarChar, 50)
            { Value = (object?)request.Phone ?? DBNull.Value });
            command.Parameters.Add(new SqlParameter("@JobTitle", SqlDbType.NVarChar, 150)
            { Value = (object?)request.JobTitle ?? DBNull.Value });
            command.Parameters.Add(
                new SqlParameter("@SystemRoleId", SqlDbType.BigInt)
                {
                    Value = Convert.ToInt64(request.SystemRoleId)
                });
            command.Parameters.Add(
                new SqlParameter("@TimeZoneId", SqlDbType.BigInt)
                {
                    Value = request.TimeZoneId
                });
            //command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit)
            //{ Value = request.IsActive });
            //command.Parameters.Add(new SqlParameter("@LastLoginUtc", SqlDbType.DateTime2)
            //{ Value = (object?)request.LastLoginUtc ?? DBNull.Value });
            command.Parameters.Add(
    new SqlParameter("@CreatedBy", SqlDbType.BigInt)
    {
        Value = Convert.ToInt64(request.CreatedBy)
    });
            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            await reader.ReadAsync(cancellationToken);

            return new CreateUserResponseDto
            {
                UserId = reader.GetInt64(reader.GetOrdinal("UserId"))
            };
        }

        public async Task<UpdateUserResponseDto> UpdateUserAsync(
            UpdateUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.User.Update,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter("@UserId", SqlDbType.BigInt)
                {
                    Value = Convert.ToInt64(request.UserId)
                });

            command.Parameters.Add(
                new SqlParameter("@SystemRoleId", SqlDbType.BigInt)
                {
                    Value = Convert.ToInt64(request.SystemRoleId)
                });

            command.Parameters.Add(
                new SqlParameter("@TimeZoneId", SqlDbType.BigInt)
                {
                    Value = request.TimeZoneId
                });

            command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit)
            { Value = request.IsActive });


            command.Parameters.Add(
                new SqlParameter("@UpdatedBy", SqlDbType.BigInt)
                {
                    Value = Convert.ToInt64(request.UpdatedBy)
                });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            await reader.ReadAsync(cancellationToken);

            return new UpdateUserResponseDto
            {
                UserId = reader.GetInt64(reader.GetOrdinal("UserId"))
            };
        }

        public async Task<DeleteUserResponseDto> DeleteUserAsync(
            DeleteUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.User.Delete,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(
                new SqlParameter("@UserId", SqlDbType.BigInt)
                {
                    Value = Convert.ToInt64(request.UserId)
                });

            command.Parameters.Add(
                new SqlParameter("@UpdatedBy", SqlDbType.BigInt)
                {
                    Value = Convert.ToInt64(request.UpdatedBy)
                });

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            await reader.ReadAsync(cancellationToken);

            return new DeleteUserResponseDto
            {
                UserId = Convert.ToInt64(request.UserId),
                IsDeleted = reader["IsDeleted"] != DBNull.Value && Convert.ToBoolean(reader["IsDeleted"]),
                HasPendingReviews = reader["HasPendingReviews"] != DBNull.Value && Convert.ToBoolean(reader["HasPendingReviews"]),
                PendingReviewIds = reader["PendingReviewIds"] != DBNull.Value
                    ? reader["PendingReviewIds"].ToString()
                    : null
            };
        }

        public async Task<IReadOnlyList<UserRoleSummaryDto>> GetRoleSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var roleSummaries = new List<UserRoleSummaryDto>();

            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.User.GetRoleSummary,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                roleSummaries.Add(new UserRoleSummaryDto
                {
                    RoleId = reader["RoleId"]?.ToString() ?? string.Empty,
                    Code = reader["Code"]?.ToString() ?? string.Empty,
                    RoleName = reader["RoleName"]?.ToString() ?? string.Empty,
                    Description = reader["Description"]?.ToString() ?? string.Empty,
                    UserCount = reader["UserCount"] != DBNull.Value
                        ? Convert.ToInt32(reader["UserCount"])
                        : 0
                });
            }

            return roleSummaries;
        }

        public async Task<IReadOnlyList<BulkImportUserResultDto>> BulkImportUsersAsync(
            IReadOnlyList<AppUserBulkImportRecordDto> users,
            string createdBy,
            CancellationToken cancellationToken = default)
        {
            var results = new List<BulkImportUserResultDto>();

            await using var connection =
                await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

            await using var command = new SqlCommand(
                StoredProcedures.User.BulkImport,
                connection);

            command.CommandType = CommandType.StoredProcedure;

            var usersTable = new DataTable();
            usersTable.Columns.Add("AzureAdObjectId", typeof(Guid));
            usersTable.Columns.Add("Upn", typeof(string));
            usersTable.Columns.Add("Email", typeof(string));
            usersTable.Columns.Add("DisplayName", typeof(string));
            usersTable.Columns.Add("FirstName", typeof(string));
            usersTable.Columns.Add("LastName", typeof(string));
            usersTable.Columns.Add("Phone", typeof(string));
            usersTable.Columns.Add("JobTitle", typeof(string));
            usersTable.Columns.Add("SystemRoleCode", typeof(string));

            foreach (var user in users)
            {
                usersTable.Rows.Add(
                    user.AzureAdObjectId,
                    user.Upn,
                    (object?)user.Email ?? DBNull.Value,
                    user.DisplayName,
                    (object?)user.FirstName ?? DBNull.Value,
                    (object?)user.LastName ?? DBNull.Value,
                    (object?)user.Phone ?? DBNull.Value,
                    (object?)user.JobTitle ?? DBNull.Value,
                    string.IsNullOrWhiteSpace(user.SystemRoleCode) ? DBNull.Value : user.SystemRoleCode);
            }

            command.Parameters.Add(
                new SqlParameter("@Users", SqlDbType.Structured)
                {
                    TypeName = "dbo.AppUserBulkModel",
                    Value = usersTable
                });

            command.Parameters.Add(
                new SqlParameter("@CreatedBy", SqlDbType.BigInt)
                {
                    Value = Convert.ToInt64(createdBy)
                });

            try
            {
                await using var reader =
                    await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    results.Add(new BulkImportUserResultDto
                    {
                        Email = reader["Email"]?.ToString() ?? string.Empty,
                        UserId = reader["UserId"] != DBNull.Value ? Convert.ToInt64(reader["UserId"]) : null,
                        Status = reader["Status"]?.ToString() ?? string.Empty,
                        Reason = reader["Reason"] != DBNull.Value ? reader["Reason"].ToString() : null
                    });
                }
            }
            catch (SqlException ex) when (ex.Number == 50001 || ex.Number == 50003)
            {
                throw new ValidationException(ex.Message);
            }

            return results;
        }

    }
}
