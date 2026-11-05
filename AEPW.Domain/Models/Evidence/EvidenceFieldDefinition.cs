namespace AEPW.Domain.Models.Evidence;

/// <summary>
/// A pre-seeded field definition for an EvidenceType, read-only from this pipeline's
/// perspective - it is looked up, never created here.
/// </summary>
public sealed class EvidenceFieldDefinition
{
    public long EvidenceFieldId { get; set; }
    public string EvidenceKey { get; set; } = string.Empty;
}
