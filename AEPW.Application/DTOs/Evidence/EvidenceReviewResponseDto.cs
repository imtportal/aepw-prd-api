namespace AEPW.Application.DTOs.Evidence;

/// <summary>
/// Response shape for GET /api/review/evidence/{evidenceId}, matching the Angular
/// evidence-review page's contract. Only sections backed by real tables are populated;
/// the rest (agentRunId, validationData[], rule counts) are stubbed - see the field
/// comments below.
/// </summary>
public sealed class EvidenceReviewResponseDto
{
    public EvidenceReviewHeaderDto HeaderData { get; set; } = new();
    public List<EvidenceReviewPageGroupDto> EvidenceDetail { get; set; } = [];
    public EvidenceReviewLineItemGroupDto LineItems { get; set; } = new();
    public EvidenceReviewEvidencedDto Evidenced { get; set; } = new();
    public EvidenceReviewValidationDto Validation { get; set; } = new();
    public EvidenceReviewRawOutputDto RawOutput { get; set; } = new();

    /// <summary>Stub - no per-rule validation engine exists yet.</summary>
    public List<object> ValidationData { get; set; } = [];
}

public sealed class EvidenceReviewHeaderDto
{
    /// <summary>Stub - no separate AgentRun entity exists yet; set to the EvidenceId as a string.</summary>
    public string SubmissionId { get; set; } = string.Empty;

    /// <summary>Stub - no AgentRun entity exists yet.</summary>
    public string? AgentRunId { get; set; }

    public string? ProjectName { get; set; }
    public string? MetricCode { get; set; }
    public string? EvidenceType { get; set; }
    public string? ReportingPeriod { get; set; }
    public string UploadedAt { get; set; } = string.Empty;
    public EvidenceReviewFileDto EvidenceFile { get; set; } = new();
    public EvidenceReviewExtractionSummaryDto ExtractionSummary { get; set; } = new();
}

public sealed class EvidenceReviewFileDto
{
    public string? FileName { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string? EvidenceUrl { get; set; }
    public string? DownloadUrl { get; set; }
    public string FileSize { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public string UploadedAt { get; set; } = string.Empty;

    /// <summary>Stub - not derivable from the stored file URLs.</summary>
    public string? BlobContainer { get; set; }

    public string SourceLabel { get; set; } = "Source Evidence";
}

public sealed class EvidenceReviewExtractionSummaryDto
{
    public string Status { get; set; } = string.Empty;
    public string StatusTitle { get; set; } = string.Empty;
    public string StatusMessage { get; set; } = string.Empty;
    public int FieldsExtracted { get; set; }

    /// <summary>Stub - no required-field metadata exists yet.</summary>
    public int RequiredFields { get; set; }

    /// <summary>Stub - no per-rule validation engine exists yet.</summary>
    public int RulesPassed { get; set; }

    /// <summary>Stub - no per-rule validation engine exists yet.</summary>
    public int Warnings { get; set; }

    /// <summary>Stub - no per-rule validation engine exists yet.</summary>
    public int Failed { get; set; }

    /// <summary>Stub - not captured by the Foundry agent output today.</summary>
    public string? AgentQuality { get; set; }
    
    public double? AvgConfidenceScore { get; set; }
}

/// <summary>One page group of evidenceDetail, holding all scalar fields extracted from that page.</summary>
public sealed class EvidenceReviewPageGroupDto
{
    public int? PageNumber { get; set; }

    /// <summary>Verified (deterministically confirmed, not raw LLM) page-duplicate result.</summary>
    public bool IsUnique { get; set; } = true;

    /// <summary>The actual earlier page this page's values matched, when IsUnique is false.</summary>
    public int? OriginalPage { get; set; }

    public List<EvidenceReviewFieldDto> Details { get; set; } = [];
}

/// <summary>One scalar (non-line-item) extracted field.</summary>
public sealed class EvidenceReviewFieldDto
{
    public long? EvidenceDetailId { get; set; } // NEW
    public long EvidenceFieldId { get; set; }   // NEW
    public string Property { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Value { get; set; }

    /// <summary>Stub - no dataType column exists on EvidenceField.</summary>
    public string DataType { get; set; } = "string";

    /// <summary>Stub - no required-field metadata exists yet.</summary>
    public string Required { get; set; } = "false";

    /// <summary>Stub - no per-rule validation engine exists yet.</summary>
    public string? ValidationCode { get; set; }
}

public sealed class EvidenceReviewLineItemGroupDto
{
    public string? FieldName { get; set; }
    public List<EvidenceReviewLineItemDto> Items { get; set; } = [];
}

public sealed class EvidenceReviewLineItemDto
{
    public long EvidenceDetailId { get; set; }
    public long EvidenceFieldId { get; set; }
    public string? Name { get; set; }
    public string? Value { get; set; }
    public double? Confidence { get; set; }
    public int? PageNumber { get; set; }
}

public sealed class EvidenceReviewEvidencedDto
{
    /// <summary>Stub - the evidenced field's name is not stored separately, only its value.</summary>
    public string? FieldName { get; set; }

    public string? EvidencedValue { get; set; }
}

public sealed class EvidenceReviewValidationDto
{
    public string? ValidationStatus { get; set; }
    public string? ValidationComment { get; set; }
    public string? IsWithinReporting { get; set; }
}

public sealed class EvidenceReviewRawOutputDto
{
    /// <summary>The stored raw agent JSON (EvidenceRawExtraction.RawJson), parsed to a JSON object; null if none stored or invalid.</summary>
    public object? Json { get; set; }
}
