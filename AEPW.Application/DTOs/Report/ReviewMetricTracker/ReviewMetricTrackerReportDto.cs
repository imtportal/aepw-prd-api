namespace AEPW.Application.DTOs.Report.ReviewMetricTracker
{
  public class ReviewMetricTrackerReportDto
  {
    public string ReportType { get; set; } = string.Empty;

    public string ReportName { get; set; } = string.Empty;

    public ReviewMetricTrackerFiltersDto Filters { get; set; }
        = new();

    public List<ReviewMetricTrackerDataDto> Data { get; set; }
        = [];

  
  }
}
