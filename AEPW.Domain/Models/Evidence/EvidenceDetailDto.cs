namespace AEPW.Domain.Models.Evidence;

/// <summary>
/// One EvidenceDetail row produced by parsing a Foundry agent's output_text, mapped to
/// a single EvidenceField definition. The owning EvidenceId is supplied separately by
/// the caller persisting the batch.
///
/// A row is either a scalar field (IsLineItem = false, Value holds the field's value,
/// LineItemValue is null) or one line item of a repeating field (IsLineItem = true,
/// Value holds the line item's name, LineItemValue holds the line item's value). All
/// line items belonging to one repeating field share the same EvidenceFieldId.
/// </summary>
public sealed class EvidenceDetailDto
{
    public long EvidenceFieldId { get; set; }
    public string? Value { get; set; }
    public string? LineItemValue { get; set; }
    public bool IsLineItem { get; set; }
    public int PageNumber { get; set; }

    /// <summary>
    /// Verified (not raw LLM) page-duplicate result: true unless this page's values were
    /// deterministically confirmed identical to an earlier page's.
    /// </summary>
    public bool IsUnique { get; set; } = true;

    /// <summary>The actual earlier page this page's values matched, when IsUnique is false.</summary>
    public int? OriginalPage { get; set; }

    public double? ConfidenceScore { get; set; }
}
