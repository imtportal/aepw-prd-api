using AEPW.Application.Exceptions;
using AEPW.Application.Interfaces.Repositories.Evidence;
using AEPW.Domain.Constants;
using AEPW.Domain.Models.Evidence;
using AEPW.Infrastructure.Configurations;
using AEPW.Infrastructure.Data;
using AEPW.Infrastructure.Helpers;
using System.Data;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace AEPW.Infrastructure.Repositories.Evidence;

public sealed class EvidenceReviewRepository : BaseRepository, IEvidenceReviewRepository
{
    /// <summary>
    /// uspSaveEvidenceReviewExtractedFields shreds @payload via OPENJSON with explicit camelCase
    /// paths ('$.evidenceDetailId', '$.value', ...) - EvidenceReviewExtractedField's PascalCase
    /// record properties must be serialized as camelCase to match, regardless of DB collation.
    /// </summary>
    private static readonly JsonSerializerOptions SaveExtractedFieldsJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public EvidenceReviewRepository(
        ISqlConnectionFactory connectionFactory,
        IOptions<DatabaseSettings> settings)
        : base(connectionFactory, settings)
    {
    }

    public async Task<EvidenceReviewData?> GetEvidenceReviewAsync(
        long evidenceId,
         long userId,
        CancellationToken cancellationToken = default)
    {
        var (connection, command) = await CreateSpCommandAsync(
            StoredProcedures.Evidence.GetReviewDetails,
            cancellationToken);

        await using var _ = connection;
        await using var __ = command;

        command.AddInputParam("@EvidenceId", System.Data.SqlDbType.BigInt, evidenceId);
        command.AddInputParam("@UserId", System.Data.SqlDbType.BigInt, userId);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        // Result set 1: header row. No row -> EvidenceId not found.
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var data = new EvidenceReviewData
        {
            EvidenceId = evidenceId,
            ReviewMetricId = reader.GetInt64(reader.GetOrdinal("ReviewMetricId")),
            FileName = reader.IsDBNull(reader.GetOrdinal("FileName")) ? null : reader.GetStringSafe("FileName"),
            FileSizeBytes = reader.IsDBNull(reader.GetOrdinal("FileSizeBytes")) ? null : reader.GetInt64(reader.GetOrdinal("FileSizeBytes")),
            FileUriView = reader.IsDBNull(reader.GetOrdinal("FileUriView")) ? null : reader.GetStringSafe("FileUriView"),
            FileUriDownload = reader.IsDBNull(reader.GetOrdinal("FileUriDownload")) ? null : reader.GetStringSafe("FileUriDownload"),
            UploadedByUserId = reader.IsDBNull(reader.GetOrdinal("UploadedByUserId")) ? null : reader.GetInt64(reader.GetOrdinal("UploadedByUserId")),
            UploadedByName = reader.IsDBNull(reader.GetOrdinal("UploadedByName")) ? null : reader.GetStringSafe("UploadedByName"),
            UploadedUtc = reader.GetDateTimeSafe("UploadedUtc"),
            Status = reader.GetInt32Safe("Status"),
            StatusCode = reader.IsDBNull(reader.GetOrdinal("StatusCode")) ? null : reader.GetStringSafe("StatusCode"),
            StatusName = reader.IsDBNull(reader.GetOrdinal("StatusName")) ? null : reader.GetStringSafe("StatusName"),
            EvidencedValue = reader.IsDBNull(reader.GetOrdinal("EvidencedValue")) ? null : reader.GetStringSafe("EvidencedValue"),
            ValidationResult = reader.IsDBNull(reader.GetOrdinal("ValidationResult")) ? null : reader.GetStringSafe("ValidationResult"),
            ReportingPeriod = reader.IsDBNull(reader.GetOrdinal("ReportingPeriod")) ? null : reader.GetStringSafe("ReportingPeriod"),
            ProjectPin = reader.IsDBNull(reader.GetOrdinal("ProjectPin")) ? null : reader.GetStringSafe("ProjectPin"),
            ProjectTitle = reader.IsDBNull(reader.GetOrdinal("ProjectTitle")) ? null : reader.GetStringSafe("ProjectTitle"),
            MetricCode = reader.IsDBNull(reader.GetOrdinal("MetricCode")) ? null : reader.GetStringSafe("MetricCode"),
            EvidenceTypeCode = reader.IsDBNull(reader.GetOrdinal("EvidenceTypeCode")) ? null : reader.GetStringSafe("EvidenceTypeCode"),
            EvidenceTypeDescription = reader.IsDBNull(reader.GetOrdinal("EvidenceTypeDescription")) ? null : reader.GetStringSafe("EvidenceTypeDescription")
        };

        // Result set 2: scalar field values (defined fields LEFT JOINed to extracted values).
        await reader.NextResultAsync(cancellationToken);

        var scalarFields = new List<EvidenceReviewFieldRow>();
        while (await reader.ReadAsync(cancellationToken))
        {
            scalarFields.Add(new EvidenceReviewFieldRow
            {
                EvidenceFieldId = reader.GetInt64(reader.GetOrdinal("EvidenceFieldId")),
                EvidenceDetailId = reader.IsDBNull(
    reader.GetOrdinal("EvidenceDetailId"))
        ? null
        : reader.GetInt64(reader.GetOrdinal("EvidenceDetailId")),
                EvidenceKey = reader.GetStringSafe("EvidenceKey"),
                Value = reader.IsDBNull(reader.GetOrdinal("Value")) ? null : reader.GetStringSafe("Value"),
                ConfidenceScore = reader.IsDBNull(reader.GetOrdinal("ConfidenceScore")) ? null : reader.GetDouble(reader.GetOrdinal("ConfidenceScore")),
                PageNumber = reader.IsDBNull(reader.GetOrdinal("PageNumber")) ? null : reader.GetInt32(reader.GetOrdinal("PageNumber")),
                IsUnique = reader.IsDBNull(reader.GetOrdinal("IsUnique")) || reader.GetBoolean(reader.GetOrdinal("IsUnique")),
                OriginalPage = reader.IsDBNull(reader.GetOrdinal("OriginalPage")) ? null : reader.GetInt32(reader.GetOrdinal("OriginalPage"))
            });
        }
        data.ScalarFields = scalarFields;

        // Result set 3: line items.
        await reader.NextResultAsync(cancellationToken);

        var lineItems = new List<EvidenceReviewLineItemRow>();
        while (await reader.ReadAsync(cancellationToken))
        {

            lineItems.Add(new EvidenceReviewLineItemRow
            {
                EvidenceDetailId = reader.GetInt64(
                    reader.GetOrdinal("EvidenceDetailId")),
                EvidenceFieldId = reader.GetInt64(
                    reader.GetOrdinal("EvidenceFieldId")),
                FieldName = reader.GetStringSafe("FieldName"),
                Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? null : reader.GetStringSafe("Name"),
                Value = reader.IsDBNull(reader.GetOrdinal("Value")) ? null : reader.GetStringSafe("Value"),
                ConfidenceScore = reader.IsDBNull(reader.GetOrdinal("Confidence")) ? null : reader.GetDouble(reader.GetOrdinal("Confidence")),
                PageNumber = reader.IsDBNull(reader.GetOrdinal("PageNumber")) ? null : reader.GetInt32(reader.GetOrdinal("PageNumber")),
                IsUnique = reader.IsDBNull(reader.GetOrdinal("IsUnique")) || reader.GetBoolean(reader.GetOrdinal("IsUnique")),
                OriginalPage = reader.IsDBNull(reader.GetOrdinal("OriginalPage")) ? null : reader.GetInt32(reader.GetOrdinal("OriginalPage"))
            });

        }
        data.LineItems = lineItems;

        // Result set 4: raw JSON (single row, or none if never processed).
        await reader.NextResultAsync(cancellationToken);

        if (await reader.ReadAsync(cancellationToken))
        {
            var rawJsonOrdinal = reader.GetOrdinal("RawJson");
            data.RawJson = reader.IsDBNull(rawJsonOrdinal) ? null : reader.GetString(rawJsonOrdinal);
        }

        return data;
    }

    public async Task SaveExtractedFieldsAsync(
        long evidenceId,
        IReadOnlyList<EvidenceReviewExtractedField> fields,
        long updatedBy,
        string? evidencedValue,
        CancellationToken cancellationToken = default)
    {
        var (connection, command) = await CreateSpCommandAsync(
            StoredProcedures.Evidence.SaveReviewExtractedFields,
            cancellationToken);

        await using var _ = connection;
        await using var __ = command;

        var payload = new
        {
            evidenceId,
            updatedBy,
            evidencedValue,
            details = fields
        };

        var payloadJson = JsonSerializer.Serialize(payload, SaveExtractedFieldsJsonOptions);

        command.AddInputParam(
            "@payload",
            SqlDbType.NVarChar,
            payloadJson);

        // The SP never lets a validation failure propagate as a real SqlException - its own
        // CATCH block converts everything (bad evidenceId, no details, etc.) into a result row
        // {Data: 0, Message: "<error>"} instead of re-throwing. ExecuteNonQueryAsync would see
        // this as an unconditional success, so the result row must be read and checked explicitly.
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new ValidationException(
                "uspSaveEvidenceReviewExtractedFields returned no result row.");
        }

        var rowsAffected = reader.GetInt32Safe("Data");
        var message = reader.GetStringSafe("Message");

        if (!string.Equals(message, "Operation Successful", StringComparison.Ordinal))
        {
            throw new ValidationException(
                $"uspSaveEvidenceReviewExtractedFields failed: {message}");
        }

        if (rowsAffected == 0)
        {
            throw new ValidationException(
                $"No EvidenceDetail rows were updated for EvidenceId={evidenceId} - the supplied EvidenceDetailId(s) may not belong to this evidence.");
        }
    }
}
