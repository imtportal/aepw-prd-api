namespace AEPW.Application.DTOs.ReviewTask;

public class GetReviewTasksRequestDto
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public int? StageCode { get; set; } = null;
    public string? PeriodCode { get; set; } = "";
    public long UserId { get; set; } = 10;
}
