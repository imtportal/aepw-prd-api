namespace AEPW.Application.DTOs.Evidence;

public sealed class ProcessEvidenceResponseDto
{
    public long EvidenceId { get; set; }
    public int FieldsExtracted { get; set; }
    public bool IsReviewRequired { get; set; }
}
