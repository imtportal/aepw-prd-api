using AEPW.Application.DTOs.Authorization;
using AEPW.Application.Interfaces.Repositories.Authorization;
using AEPW.Domain.Constants;
using AEPW.Domain.Models.Authorization;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AEPW.Infrastructure.Repositories.Authorization;

public sealed class RoleAccessRepository : IRoleAccessRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public RoleAccessRepository(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<IReadOnlyList<RolePageAccessModel>> GetRolePageAccessAsync(
        CancellationToken cancellationToken = default)
    {
        var results = new List<RolePageAccessModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.RoleAccess.GetRolePageAccess,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new RolePageAccessModel
            {
                SystemRoleId = reader.GetInt64(reader.GetOrdinal("SystemRoleId")),
                SystemRoleName = reader["SystemRoleName"]?.ToString() ?? string.Empty,
                PageStructureId = reader.GetInt64(reader.GetOrdinal("PageStructureId")),
                PageName = reader["PageName"]?.ToString() ?? string.Empty,
                Description = reader["Description"]?.ToString() ?? string.Empty,
                HasAccess = reader["HasAccess"] != DBNull.Value && Convert.ToBoolean(reader["HasAccess"])
            });
        }

        return results;
    }

    public async Task<IReadOnlyList<RolePageActionAccessModel>> GetRolePageActionAccessAsync(
        long pageStructureId,
        CancellationToken cancellationToken = default)
    {
        var results = new List<RolePageActionAccessModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.RoleAccess.GetRolePageActionAccess,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@PageStructureId", SqlDbType.BigInt)
        {
            Value = pageStructureId
        });

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new RolePageActionAccessModel
            {
                SystemRoleId = reader.GetInt64(reader.GetOrdinal("SystemRoleId")),
                SystemRoleName = reader["SystemRoleName"]?.ToString() ?? string.Empty,
                PageStructureId = reader.GetInt64(reader.GetOrdinal("PageStructureId")),
                PageActionStructureId = reader.GetInt64(reader.GetOrdinal("PageActionStructureId")),
                ActionName = reader["ActionName"]?.ToString() ?? string.Empty,
                ActionKey = reader["ActionKey"]?.ToString() ?? string.Empty,
                ActionDescription = reader["ActionDescription"]?.ToString() ?? string.Empty,
                HasAccess = reader["HasAccess"] != DBNull.Value && Convert.ToBoolean(reader["HasAccess"])
            });
        }

        return results;
    }

    public async Task SaveRolePageAccessAsync(
        SaveRolePageAccessRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.RoleAccess.SaveRolePageAccess,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        // Build TVP for RolePageAccess - entire matrix in one DataTable
        var rolePageAccessTable = new DataTable();
        rolePageAccessTable.Columns.Add("SystemRoleId", typeof(long));
        rolePageAccessTable.Columns.Add("PageStructureId", typeof(long));
        rolePageAccessTable.Columns.Add("HasAccess", typeof(bool));

        foreach (var item in request.RolePageAccess)
        {
            rolePageAccessTable.Rows.Add(item.SystemRoleId, item.PageStructureId, item.HasAccess);
        }

        var tvpParam = new SqlParameter("@RolePageAccess", SqlDbType.Structured)
        {
            TypeName = "dbo.RolePageAccessType",
            Value = rolePageAccessTable
        };
        command.Parameters.Add(tvpParam);

        command.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.BigInt)
        {
            Value = request.UpdatedBy
        });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task SaveRolePageActionAccessAsync(
        SaveRolePageActionAccessRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.RoleAccess.SaveRolePageActionAccess,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        // Build TVP for RolePageActionAccess - entire matrix in one DataTable
        var rolePageActionAccessTable = new DataTable();
        rolePageActionAccessTable.Columns.Add("SystemRoleId", typeof(long));
        rolePageActionAccessTable.Columns.Add("PageActionStructureId", typeof(long));
        rolePageActionAccessTable.Columns.Add("CanPerformAction", typeof(bool));

        foreach (var item in request.RolePageActionAccess)
        {
            rolePageActionAccessTable.Rows.Add(item.SystemRoleId, item.PageActionStructureId, item.CanPerformAction);
        }

        var tvpParam = new SqlParameter("@RolePageActionAccess", SqlDbType.Structured)
        {
            TypeName = "dbo.RolePageActionAccessType",
            Value = rolePageActionAccessTable
        };
        command.Parameters.Add(tvpParam);

        command.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.BigInt)
        {
            Value = request.UpdatedBy
        });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
