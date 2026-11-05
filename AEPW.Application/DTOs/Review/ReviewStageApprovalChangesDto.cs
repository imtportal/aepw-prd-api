namespace AEPW.Application.DTOs.Review;

/// <summary>Approval changes grouped under one review stage.</summary>
public sealed record ReviewStageApprovalChangesDto(
    int StageCode,
    string StageName,
    IReadOnlyList<ReviewApprovalChangeRecordDto> Changes);
