namespace AEPW.Application.DTOs.Review;

/// <summary>Identifies a review stage and its target active state for an approval preview.</summary>
public sealed record ReviewStageApprovalChangeRequestDto
{
    public required int StageCode { get; init; }
    public required bool IsActive { get; init; }
}
