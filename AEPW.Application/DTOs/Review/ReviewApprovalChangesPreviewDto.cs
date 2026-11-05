namespace AEPW.Application.DTOs.Review;

/// <summary>Read-only approval previews for stages being disabled or enabled.</summary>
public sealed record ReviewApprovalChangesPreviewDto(
    IReadOnlyList<ReviewStageApprovalChangesDto> DisablePreview,
    IReadOnlyList<ReviewStageApprovalChangesDto> EnablePreview);
