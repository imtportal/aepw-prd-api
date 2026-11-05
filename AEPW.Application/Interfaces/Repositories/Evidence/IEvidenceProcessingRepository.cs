using AEPW.Domain.Models.Evidence;

namespace AEPW.Application.Interfaces.Repositories.Evidence;

/// <summary>
/// Data access for the evidence-processing pipeline (type/field lookups, and
/// persisting extraction results). All calls are stored-procedure backed.
/// </summary>
public interface IEvidenceProcessingRepository
{
    /// <summary>
    /// Resolves an EvidenceType by its Code (dbo.EvidenceType.Code). Returns null when not
    /// found/inactive. NOT metric-scoped - EvidenceType.Code is the sole lookup key.
    /// </summary>
    Task<EvidenceTypeLookupResult?> GetEvidenceTypeByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the MetricEvidenceTypeId (the Metric+EvidenceType junction row) for an Evidence
    /// row - MetricId comes from the evidence's own ReviewMetric; <paramref name="evidenceTypeId"/>
    /// is the already-resolved EvidenceTypeId (from <see cref="GetEvidenceTypeByCodeAsync"/>), not
    /// read from Evidence.EvidenceTypeId, which is not reliably populated at process-time. The
    /// same EvidenceType is attached to multiple metrics through MetricEvidenceType, each with its
    /// own seeded field set - EvidenceTypeId alone is not enough to load the right fields.
    /// Returns null when not found.
    /// </summary>
    Task<long?> GetMetricEvidenceTypeIdByEvidenceIdAsync(
        long evidenceId,
        long evidenceTypeId,
        CancellationToken cancellationToken = default);

    /// <summary>Loads the pre-seeded field definitions for a MetricEvidenceType.</summary>
    Task<IReadOnlyList<EvidenceFieldDefinition>> GetFieldsByTypeIdAsync(
        long metricEvidenceTypeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces all EvidenceDetail rows for the given EvidenceId with <paramref name="details"/>.
    /// </summary>
    Task SaveDetailsAsync(
        long evidenceId,
        IReadOnlyList<EvidenceDetailDto> details,
        long createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>Upserts the raw Document Intelligence model response for an EvidenceId.</summary>
    Task SaveRawExtractionAsync(
        long evidenceId,
        string rawJson,
        string modelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the Evidence row's review flag, status, evidenced value, validation
    /// result, and IsWithinReporting flag after processing. <paramref name="statusCode"/>
    /// must be a valid EVIDENCE-module StatusCode from MasterStatus (see
    /// <see cref="AEPW.Domain.Constants.EvidenceStatuses"/>) - the stored procedure
    /// resolves it to the int StatusId stored on Evidence.Status.
    /// </summary>
    Task UpdateProcessingResultAsync(
        long evidenceId,
        bool isReviewRequired,
        string statusCode,
        string? evidencedValue,
        string? validationResult,
        bool isWithinReporting,
        long updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Directly sets Evidence.Status to a raw StatusId (see
    /// <see cref="AEPW.Domain.Constants.EvidenceStatusIds"/>), bypassing the
    /// StatusCode-to-StatusId resolution <see cref="UpdateProcessingResultAsync"/> does.
    /// Used to bracket a /process request: Processing on start, Failed on any error.
    /// </summary>
    Task SetStatusAsync(
        long evidenceId,
        int statusId,
        long updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>Resolves the owning ReviewId for an Evidence row, via its ReviewMetric. Returns null when not found.</summary>
    Task<long?> GetReviewIdByEvidenceIdAsync(
        long evidenceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recomputes top-metric adjusted values for a Review. Best-effort - callers should
    /// treat any failure from this call as non-fatal to the evidence-processing pipeline.
    /// Returns the SP's (Data, Message) result row, if any, for logging.
    /// </summary>
    Task<(string? Data, string? Message)> UpdateRawAdjustedValuesForTopMetricsAsync(
        long reviewId,
        CancellationToken cancellationToken = default);
}
