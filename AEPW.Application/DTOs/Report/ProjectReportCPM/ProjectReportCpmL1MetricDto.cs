namespace AEPW.Application.DTOs.Report.ProjectReportCpm
{
    public class ProjectReportCpmL1MetricDto
    {
        public string MetricCode { get; set; } = string.Empty;

        public string MetricName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public List<ProjectReportCpmYearlyDataDto> YearlyData { get; set; }
            = [];

        public List<ProjectReportCpmYearlyDataDto> AggregationData { get; set; }
    = new();
    }
}
