namespace AEPW.Application.DTOs.Evidence;

/// <summary>Request to save edited scalar fields in an Evidence Review.</summary>
public sealed record SaveEvidenceReviewExtractedFieldsRequestDto
{
    /// <summary>Evidence record identifier; must match the route identifier.</summary>
    public long EvidenceId { get; init; }

    /// <summary>All edited scalar fields across the evidence document pages.</summary>
    public IReadOnlyList<EvidenceReviewExtractedFieldRequestDto?>? Fields { get; init; }

    /// <summary>Evidenced value for the evidence record.</summary>
    public string? EvidencedValue { get; init; }

    /// <summary>Authenticated application user identifier supplied by the caller.</summary>
    public long UpdatedBy { get; init; }
}

/// <summary>A scalar extracted field identified by its document page and existing property.</summary>
public sealed record EvidenceReviewExtractedFieldRequestDto
{
    public long EvidenceDetailId { get; init; }
    /// <summary>One-based document page number.</summary>
    public int PageNumber { get; init; }

    /// <summary>Existing property key from the Evidence Review field definition.</summary>
    //public string? Property { get; init; }

    /// <summary>Edited value; null clears the value.</summary>
    public string? Value { get; init; }
    public string? LineItemValue { get; init; }
}
