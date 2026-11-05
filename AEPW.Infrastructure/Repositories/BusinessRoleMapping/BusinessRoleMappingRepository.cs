using AEPW.Application.Interfaces.Repositories.BusinessRoleMapping;
using AEPW.Domain.Constants;
using AEPW.Domain.Models.BusinessRoleMapping;
using AEPW.Domain.Models.Lookup;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AEPW.Infrastructure.Repositories.BusinessRoleMapping;

public sealed class BusinessRoleMappingRepository : IBusinessRoleMappingRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public BusinessRoleMappingRepository(
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }
    public async Task<IReadOnlyList<BusinessRoleMappingLookupModel>> GetAllBusinessRoleMappingsAsync(
    long? userId, CancellationToken cancellationToken = default)
    {
        var results = new List<BusinessRoleMappingLookupModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetBusinessRoleMappings,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.Add(
    "@UserId",
    SqlDbType.BigInt
).Value = userId ?? (object)DBNull.Value;

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new BusinessRoleMappingLookupModel
            {
                BusinessRoleMappingId = reader["BusinessRoleMappingId"].ToString()!,

                UserId = reader["UserId"].ToString()!,
                UserName = reader["UserName"]?.ToString() ?? string.Empty,

                BusinessRoleId = reader["BusinessRoleId"].ToString()!,
                BusinessRoleName = reader["BusinessRoleName"]?.ToString() ?? string.Empty,
                RoleCode = reader["RoleCode"]?.ToString() ?? string.Empty,

                RegionId = reader["RegionId"] == DBNull.Value
                    ? null
                    : reader["RegionId"].ToString(),

                RegionCode = reader["RegionCode"] == DBNull.Value
                    ? null
                    : reader["RegionCode"].ToString(),

                RegionName = reader["RegionName"] == DBNull.Value
                    ? null
                    : reader["RegionName"].ToString(),

                ReviewStageCode = reader["ReviewStageCode"] == DBNull.Value
                    ? null
                    : reader["ReviewStageCode"].ToString(),

                ReviewStageName = reader["ReviewStageName"] == DBNull.Value
                    ? null
                    : reader["ReviewStageName"].ToString(),

                CategoryCode = reader["CategoryCode"] == DBNull.Value
                    ? null
                    : reader["CategoryCode"].ToString(),

                CategoryName = reader["CategoryName"] == DBNull.Value
                    ? null
                    : reader["CategoryName"].ToString(),

                CategoryColor = reader["CategoryColor"] == DBNull.Value
                    ? null
                    : reader["CategoryColor"].ToString(),

                IsActive = Convert.ToBoolean(reader["IsActive"])
            });
        }

        return results;
    }
    public async Task<string> SaveBusinessRoleMappingAsync(
        SaveBusinessRoleMappingModel model,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.SaveBusinessRoleMapping,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue(
            "@BusinessRoleMappingId",
            string.IsNullOrWhiteSpace(model.BusinessRoleMappingId)
                ? DBNull.Value
                : Convert.ToInt64(model.BusinessRoleMappingId));

        command.Parameters.AddWithValue(
            "@BusinessRoleId",
            Convert.ToInt64(model.BusinessRoleId));

        command.Parameters.AddWithValue(
            "@UserId",
            Convert.ToInt64(model.UserId));

        command.Parameters.AddWithValue(
            "@RegionId",
            string.IsNullOrWhiteSpace(model.RegionId)
                ? DBNull.Value
                : Convert.ToInt64(model.RegionId));

        command.Parameters.AddWithValue(
            "@ReviewStageCode",
            string.IsNullOrWhiteSpace(model.ReviewStageCode)
                ? DBNull.Value
                : Convert.ToInt64(model.ReviewStageCode));

        command.Parameters.AddWithValue(
            "@CategoryCode",
            string.IsNullOrWhiteSpace(model.CategoryCode)
                ? DBNull.Value
                : model.CategoryCode);

        command.Parameters.AddWithValue(
            "@IsActive",
            model.IsActive);

        command.Parameters.AddWithValue(
            "@CreatedBy",
            Convert.ToInt64(model.CreatedBy));

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result?.ToString() ?? string.Empty;
    }

    public async Task<BusinessRoleMappingDetailModel?> GetBusinessRoleMappingByIdAsync(
        string businessRoleMappingId,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.GetBusinessRoleMappingById,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue(
            "@BusinessRoleMappingId",
            Convert.ToInt64(businessRoleMappingId));

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new BusinessRoleMappingDetailModel
        {
            BusinessRoleMappingId = reader["BusinessRoleMappingId"].ToString()!,

            UserId = reader["UserId"].ToString()!,
            UserName = reader["UserName"]?.ToString() ?? string.Empty,

            BusinessRoleId = reader["BusinessRoleId"].ToString()!,
            BusinessRoleName = reader["BusinessRoleName"]?.ToString() ?? string.Empty,
            RoleCode = reader["RoleCode"]?.ToString() ?? string.Empty,

            RegionId = reader["RegionId"]?.ToString(),
            RegionCode = reader["RegionCode"]?.ToString(),
            RegionName = reader["RegionName"]?.ToString(),

            ReviewStageCode = reader["ReviewStageCode"]?.ToString(),
            ReviewStageName = reader["ReviewStageName"]?.ToString(),

            CategoryCode = reader["CategoryCode"]?.ToString(),
            CategoryName = reader["CategoryName"]?.ToString(),
            CategoryColor = reader["CategoryColor"]?.ToString(),

            IsActive = Convert.ToBoolean(reader["IsActive"])
        };
    }

    public async Task<bool> DeleteBusinessRoleMappingAsync(
        DeleteBusinessRoleMappingModel model,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(
                cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Lookup.DeleteBusinessRoleMapping,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(
            "@BusinessRoleMappingId",
            SqlDbType.BigInt).Value = model.BusinessRoleMappingId;

        command.Parameters.Add(
            "@UpdatedBy",
            SqlDbType.BigInt).Value = model.UpdatedBy;

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result != null
            && result != DBNull.Value
            && Convert.ToBoolean(result);
    }

}