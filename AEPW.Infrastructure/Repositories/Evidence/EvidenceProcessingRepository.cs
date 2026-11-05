using System.Data;
using System.Text.Json;
using AEPW.Application.Exceptions;
using AEPW.Application.Interfaces.Repositories.Evidence;
using AEPW.Domain.Constants;
using AEPW.Domain.Models.Evidence;
using AEPW.Infrastructure.Configurations;
using AEPW.Infrastructure.Data;
using AEPW.Infrastructure.Helpers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace AEPW.Infrastructure.Repositories.Evidence;

public sealed class EvidenceProcessingRepository : BaseRepository, IEvidenceProcessingRepository
{
    /// <summary>
    /// SQL error numbers that are transient (deadlock victim, command timeout, Azure SQL
    /// connection-resiliency/throttling codes) - a retry of the SAME write may succeed.
    /// Anything else from these final-write calls is treated as permanent.
    /// </summary>
    private static readonly HashSet<int> TransientSqlErrorNumbers = new()
    {
        1205,   // deadlock victim
        -2,     // command/connection timeout
        40197, 40501, 49918, 49919, 49920 // Azure SQL throttling / failover / resource governance
    };

    public EvidenceProcessingRepository(
        ISqlConnectionFactory connectionFactory,
        IOptions<DatabaseSettings> settings)
        : base(connectionFactory, settings)
    {
    }

    public async Task<EvidenceTypeLookupResult?> GetEvidenceTypeByCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        var (connection, command) = await CreateSpCommandAsync(
            StoredProcedures.Evidence.GetTypeByCode,
            cancellationToken);

        await using var _ = connection;
        await using var __ = command;

        command.AddInputParam("@Code", SqlDbType.NVarChar, code);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new EvidenceTypeLookupResult
        {
            EvidenceTypeId = reader.GetInt64(reader.GetOrdinal("EvidenceTypeId")),
            IsExchangeRateRequired = reader.GetBooleanSafe("IsExchangeRateRequired")
        };
    }

    public async Task<long?> GetMetricEvidenceTypeIdByEvidenceIdAsync(
        long evidenceId,
        long evidenceTypeId,
        CancellationToken cancellationToken = default)
    {
        var (connection, command) = await CreateSpCommandAsync(
            StoredProcedures.Evidence.GetMetricEvidenceTypeIdByEvidenceId,
            cancellationToken);

        await using var _ = connection;
        await using var __ = command;

        command.AddInputParam("@EvidenceId", SqlDbType.BigInt, evidenceId);
        command.AddInputParam("@EvidenceTypeId", SqlDbType.BigInt, evidenceTypeId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var ordinal = reader.GetOrdinal("MetricEvidenceTypeId");
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }

    public async Task<IReadOnlyList<EvidenceFieldDefinition>> GetFieldsByTypeIdAsync(
        long metricEvidenceTypeId,
        CancellationToken cancellationToken = default)
    {
        var (connection, command) = await CreateSpCommandAsync(
            StoredProcedures.Evidence.GetFieldsByTypeId,
            cancellationToken);

        await using var _ = connection;
        await using var __ = command;

        command.AddInputParam("@metricEvidenceTypeId", SqlDbType.BigInt, metricEvidenceTypeId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var fields = new List<EvidenceFieldDefinition>();

        while (await reader.ReadAsync(cancellationToken))
        {
            fields.Add(new EvidenceFieldDefinition
            {
                EvidenceFieldId = reader.GetInt64(reader.GetOrdinal("EvidenceFieldId")),
                EvidenceKey = reader.GetStringSafe("EvidenceKey")
            });
        }

        return fields;
    }

    public async Task SaveDetailsAsync(
        long evidenceId,
        IReadOnlyList<EvidenceDetailDto> details,
        long createdBy,
        CancellationToken cancellationToken = default)
    {
        var (connection, command) = await CreateSpCommandAsync(
            StoredProcedures.Evidence.SaveDetails,
            cancellationToken);

        await using var _ = connection;
        await using var __ = command;

        command.AddInputParam("@EvidenceId", SqlDbType.BigInt, evidenceId);

        var detailsJson = JsonSerializer.Serialize(details.Select(d => new
        {
            d.EvidenceFieldId,
            d.Value,
            d.LineItemValue,
            d.IsLineItem,
            d.PageNumber,
            d.IsUnique,
            d.OriginalPage,
            d.ConfidenceScore
        }));

        command.AddInputParam("@DetailsJson", SqlDbType.NVarChar, detailsJson);
        command.AddInputParam("@CreatedBy", SqlDbType.BigInt, createdBy);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex) when (TransientSqlErrorNumbers.Contains(ex.Number))
        {
            throw new TransientProcessingException($"Transient SQL error ({ex.Number}) saving evidence details.", ex);
        }
        catch (SqlException ex) when (ex.Number is 51002)
        {
            throw new ValidationException(ex.Message);
        }
    }

    public async Task SaveRawExtractionAsync(
        long evidenceId,
        string rawJson,
        string modelId,
        CancellationToken cancellationToken = default)
    {
        var (connection, command) = await CreateSpCommandAsync(
            StoredProcedures.Evidence.SaveRawExtraction,
            cancellationToken);

        await using var _ = connection;
        await using var __ = command;

        command.AddInputParam("@EvidenceId", SqlDbType.BigInt, evidenceId);
        command.AddInputParam("@RawJson", SqlDbType.NVarChar, rawJson);
        command.AddInputParamNullable("@ModelId", SqlDbType.NVarChar, modelId);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex) when (TransientSqlErrorNumbers.Contains(ex.Number))
        {
            throw new TransientProcessingException($"Transient SQL error ({ex.Number}) saving raw extraction JSON.", ex);
        }
    }

    public async Task UpdateProcessingResultAsync(
        long evidenceId,
        bool isReviewRequired,
        string statusCode,
        string? evidencedValue,
        string? validationResult,
        bool isWithinReporting,
        long updatedBy,
        CancellationToken cancellationToken = default)
    {
        var (connection, command) = await CreateSpCommandAsync(
            StoredProcedures.Evidence.UpdateProcessingResult,
            cancellationToken);

        await using var _ = connection;
        await using var __ = command;

        command.AddInputParam("@EvidenceId", SqlDbType.BigInt, evidenceId);
        command.AddInputParam("@IsReviewRequired", SqlDbType.Bit, isReviewRequired);
        command.AddInputParam("@StatusCode", SqlDbType.NVarChar, statusCode);
        command.AddInputParamNullable("@EvidencedValue", SqlDbType.NVarChar, evidencedValue);
        command.AddInputParamNullable("@ValidationResult", SqlDbType.NVarChar, validationResult);
        command.AddInputParam("@IsWithinReporting", SqlDbType.Bit, isWithinReporting);
        command.AddInputParam("@UpdatedBy", SqlDbType.BigInt, updatedBy);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex) when (TransientSqlErrorNumbers.Contains(ex.Number))
        {
            throw new TransientProcessingException($"Transient SQL error ({ex.Number}) updating evidence processing result.", ex);
        }
        catch (SqlException ex) when (ex.Number is 51001)
        {
            throw new NotFoundException("Evidence", evidenceId);
        }
        catch (SqlException ex) when (ex.Number is 51003)
        {
            throw new ValidationException(ex.Message);
        }
    }

    public async Task SetStatusAsync(
        long evidenceId,
        int statusId,
        long updatedBy,
        CancellationToken cancellationToken = default)
    {
        var (connection, command) = await CreateSpCommandAsync(
            StoredProcedures.Evidence.SetStatus,
            cancellationToken);

        await using var _ = connection;
        await using var __ = command;

        command.AddInputParam("@EvidenceId", SqlDbType.BigInt, evidenceId);
        command.AddInputParam("@StatusId", SqlDbType.Int, statusId);
        command.AddInputParam("@UpdatedBy", SqlDbType.BigInt, updatedBy);

        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (SqlException ex) when (ex.Number is 51001)
        {
            throw new NotFoundException("Evidence", evidenceId);
        }
    }

    public async Task<long?> GetReviewIdByEvidenceIdAsync(
        long evidenceId,
        CancellationToken cancellationToken = default)
    {
        var (connection, command) = await CreateSpCommandAsync(
            StoredProcedures.Evidence.GetReviewIdByEvidenceId,
            cancellationToken);

        await using var _ = connection;
        await using var __ = command;

        command.AddInputParam("@EvidenceId", SqlDbType.BigInt, evidenceId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var ordinal = reader.GetOrdinal("ReviewId");
        return reader.IsDBNull(ordinal) ? null : reader.GetInt64(ordinal);
    }

    public async Task<(string? Data, string? Message)> UpdateRawAdjustedValuesForTopMetricsAsync(
        long reviewId,
        CancellationToken cancellationToken = default)
    {
        var (connection, command) = await CreateSpCommandAsync(
            StoredProcedures.Evidence.UpdateRawAdjustedValuesForTopMetrics,
            cancellationToken);

        await using var _ = connection;
        await using var __ = command;

        // The real procedure takes @reviewIds NVARCHAR(MAX) - a comma-separated list, split
        // internally via string_split (it supports recomputing several reviews in one call).
        // This call site only ever recomputes the one review just processed, so a single value
        // is passed as a one-element "list". @SkipL1Calculation is left unset to use the
        // procedure's own default (0 = always recalculate L1 AdjustedValue).
        command.AddInputParam("@reviewIds", SqlDbType.NVarChar, reviewId.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        // On success the procedure returns NO result set at all (its TRY block never SELECTs);
        // a row only comes back from the CATCH block (Data=0, Message='ln X: <error>').
        if (reader.HasRows && await reader.ReadAsync(cancellationToken))
        {
            return (reader["Data"]?.ToString(), reader["Message"]?.ToString());
        }

        return (null, null);
    }
}
