namespace AEPW.Domain.Models.Evidence;

/// <summary>
/// Raw data for one Evidence row read by <c>dbo.uspEvidenceGetReviewDetails</c>, before
/// the service layer maps it onto the Angular-facing EvidenceReviewResponseDto shape.
/// </summary>
public sealed class EvidenceReviewData
{
    public long EvidenceId { get; set; }
    public long ReviewMetricId { get; set; }
    public string? FileName { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? FileUriView { get; set; }
    public string? FileUriDownload { get; set; }
    public long? UploadedByUserId { get; set; }
    public string? UploadedByName { get; set; }
    public DateTime UploadedUtc { get; set; }
    public int Status { get; set; }
    public string? StatusCode { get; set; }
    public string? StatusName { get; set; }
    public string? EvidencedValue { get; set; }
    public string? ValidationResult { get; set; }
    public string? ReportingPeriod { get; set; }
    public string? ProjectPin { get; set; }
    public string? ProjectTitle { get; set; }
    public string? MetricCode { get; set; }
    public string? EvidenceTypeCode { get; set; }
    public string? EvidenceTypeDescription { get; set; }
    public IReadOnlyList<EvidenceReviewFieldRow> ScalarFields { get; set; } = [];
    public IReadOnlyList<EvidenceReviewLineItemRow> LineItems { get; set; } = [];
    public string? RawJson { get; set; }
}

/// <summary>One defined scalar EvidenceField, left-joined to its extracted value (if any).</summary>
public sealed class EvidenceReviewFieldRow
{
    public long EvidenceFieldId { get; set; }
    // NEW: Identifies the exact extracted database record.
    public long? EvidenceDetailId { get; set; }
    public string EvidenceKey { get; set; } = string.Empty;
    public string? Value { get; set; }
    public double? ConfidenceScore { get; set; }
    public int? PageNumber { get; set; }

    /// <summary>Verified page-duplicate result (defaults true when no EvidenceDetail row exists for this field).</summary>
    public bool IsUnique { get; set; } = true;
    public int? OriginalPage { get; set; }
}

/// <summary>One line-item row (IsLineItem = 1) for the evidence's line-item field group.</summary>
public sealed class EvidenceReviewLineItemRow
{
    public long EvidenceDetailId { get; set; }
    public long EvidenceFieldId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Value { get; set; }
    public double? ConfidenceScore { get; set; }
    public int? PageNumber { get; set; }
    public bool IsUnique { get; set; } = true;
    public int? OriginalPage { get; set; }
}
