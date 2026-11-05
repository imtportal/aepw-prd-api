namespace AEPW.Application.DTOs.Review;

public sealed record ReviewApprovalChangeRepositoryRowDto(
    int StageCode,
    string StageName,
    long? ReviewApprovalId,
    long ReviewId,
    string ReviewTitle,
    long MetricId,
    string MetricCode,
    string MetricName,
    int? StatusId,
    string? StatusCode,
    string? CurrentStatus,
    long? AssigneeUserId,
    int? StepNo,
    int? AttemptNo,
    string Action);
