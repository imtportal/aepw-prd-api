namespace AEPW.Application.DTOs.Review;

/// <summary>Represents an approval affected by changing a review stage.</summary>
public sealed record ReviewApprovalChangeRecordDto(
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
