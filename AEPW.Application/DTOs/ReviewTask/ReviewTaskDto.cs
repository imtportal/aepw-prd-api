namespace AEPW.Application.DTOs.ReviewTask;

public class ReviewTaskDto
{
    public long ReviewId { get; set; }

    public long ImpactMasterId { get; set; }

    public string Pin { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public long RecordTypeId { get; set; }

    public string RecordTypeName { get; set; } = string.Empty;

    public long StatusId { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public string? PeriodCode { get; set; }

    public int Stage { get; set; }

    public string StageName { get; set; } = string.Empty;
    public string? StageDisplayValue { get; set; } = string.Empty;

    public string? MetricCategories { get; set; }

    public DateTime CreatedOnUTC { get; set; }

    public DateTime? UpdatedOnUTC { get; set; }
}
