namespace AEPW.Domain.Models.Evidence;

/// <summary>An edited scalar field passed to the Evidence Review persistence layer.</summary>
//public sealed record EvidenceReviewExtractedField(
//    int PageNumber,
//    string Property,
//    string? Value);

public sealed record EvidenceReviewExtractedField(
    long EvidenceDetailId,
    long EvidenceFieldId,
    string? Value,
    string? LineItemValue,
    bool IsLineItem,
    int? PageNumber,
    double? ConfidenceScore);
