namespace AEPW.Application.DTOs.Report.ProjectReportCpm
{
    public class ProjectReportCpmMetricDto
    {
  

    public long MetricId { get; set; }

    public string MetricCode { get; set; } = string.Empty;

    public string MetricName { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public List<ProjectReportCpmMetricDto> L1Metrics { get; set; }
        = [];

    public List<ProjectReportCpmYearlyDataDto> YearlyData { get; set; }
        = [];
  }
}
