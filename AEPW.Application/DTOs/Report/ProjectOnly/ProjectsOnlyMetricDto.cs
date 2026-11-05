namespace AEPW.Application.DTOs.Report.ProjectsOnly
{
    public class ProjectsOnlyMetricDto
    {
        public string MetricCode { get; set; } = string.Empty;

        public string MetricName { get; set; } = string.Empty;

        public ProjectsOnlyQuarterDataDto OriginalData { get; set; } = new();

        public ProjectsOnlyQuarterDataDto RiskAdjustedData { get; set; } = new();
    }
}
