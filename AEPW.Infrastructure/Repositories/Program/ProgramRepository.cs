using AEPW.Application.DTOs.Program;
using AEPW.Application.Interfaces.Repositories.Program;
using AEPW.Domain.Constants;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;
using AEPW.Application.Contracts.Responses;

namespace AEPW.Infrastructure.Repositories.Program;

public sealed class ProgramRepository : IProgramRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public ProgramRepository(
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<PagedResponse<ProgramDto>> GetAllProgramsAsync(
      GetProgramsRequestDto request,
      CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(
                cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Program.GetAll,
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@PageNumber", request.PageNumber);
        command.Parameters.AddWithValue("@PageSize", request.PageSize);
        command.Parameters.AddWithValue(
            "@SearchText",
            string.IsNullOrWhiteSpace(request.SearchText)
                ? DBNull.Value
                : request.SearchText);
        command.Parameters.AddWithValue(
    "@ProgramId",
    request.ProgramId.HasValue
        ? request.ProgramId.Value
        : DBNull.Value);
        command.Parameters.AddWithValue(
    "@IsActive",
    request.IsActive.HasValue
        ? request.IsActive.Value
        : DBNull.Value);
        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var programs = new List<ProgramDto>();
        int totalRecords = 0;

        while (await reader.ReadAsync(cancellationToken))
        {
            if (totalRecords == 0)
            {
                totalRecords = reader.GetInt32(
                    reader.GetOrdinal("TotalRecords"));
            }

            programs.Add(new ProgramDto
            {
                ProgramId = reader.GetInt64(reader.GetOrdinal("ProgramId")),

                Code = reader["Code"]?.ToString() ?? string.Empty,

                Name = reader["Name"]?.ToString() ?? string.Empty,

                Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                    ? null
                    : reader["Description"]?.ToString(),

                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            });
        }

        return new PagedResponse<ProgramDto>
        {
            Items = programs,
            TotalRecords = totalRecords,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<ProgramDetailDto?> GetProgramByIdAsync(
    long programId,
    CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(
                cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Program.GetById,
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue(
            "@ProgramId",
            programId);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new ProgramDetailDto
        {
            ProgramId = reader.GetInt64(
                reader.GetOrdinal("ProgramId")),

            Code = reader["Code"]?.ToString() ?? string.Empty,

            Name = reader["Name"]?.ToString() ?? string.Empty,

            Description = reader.IsDBNull(
                reader.GetOrdinal("Description"))
                    ? null
                    : reader["Description"]?.ToString(),

            IsActive = reader.GetBoolean(
                reader.GetOrdinal("IsActive")),

            CreatedBy = reader.GetInt64(
                reader.GetOrdinal("CreatedBy")),

            CreatedOnUTC = reader.GetDateTime(
                reader.GetOrdinal("CreatedOnUTC")),

            UpdatedBy = reader.IsDBNull(
                reader.GetOrdinal("UpdatedBy"))
                    ? null
                    : reader.GetInt64(
                        reader.GetOrdinal("UpdatedBy")),

            UpdatedOnUTC = reader.IsDBNull(
                reader.GetOrdinal("UpdatedOnUTC"))
                    ? null
                    : reader.GetDateTime(
                        reader.GetOrdinal("UpdatedOnUTC"))
        };
    }

    public async Task InsertProgramAsync(
    InsertProgramRequestDto request,
    CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(
                cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Program.Insert,
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue(
            "@Code",
            request.Code);

        command.Parameters.AddWithValue(
            "@Name",
            request.Name);

        command.Parameters.AddWithValue(
            "@Description",
            (object?)request.Description ?? DBNull.Value);

        command.Parameters.AddWithValue(
    "@IsActive",
    request.IsActive);

        command.Parameters.AddWithValue(
            "@CreatedBy",
            request.CreatedBy);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateProgramAsync(
    UpdateProgramRequestDto request,
    CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(
                cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.Program.UpdateById,
            connection);

        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue(
            "@ProgramId",
            request.ProgramId);

        command.Parameters.AddWithValue(
            "@Code",
            request.Code);

        command.Parameters.AddWithValue(
            "@Name",
            request.Name);

        command.Parameters.AddWithValue(
            "@Description",
            (object?)request.Description ?? DBNull.Value);

        command.Parameters.AddWithValue(
            "@IsActive",
            request.IsActive);

        command.Parameters.AddWithValue(
            "@UpdatedBy",
            request.UpdatedBy);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }


}
