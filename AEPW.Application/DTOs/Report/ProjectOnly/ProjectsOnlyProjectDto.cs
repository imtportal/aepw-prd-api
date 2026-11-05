namespace AEPW.Application.DTOs.Report.ProjectsOnly
{
    public class ProjectsOnlyProjectDto
    {
        public string ImId { get; set; } = string.Empty;

      public string Pin { get; set; } = string.Empty;

        public string ProjectTitle { get; set; } = string.Empty;
        public string ReportingPeriod { get; set; } = string.Empty;

        public string ContractPerformanceManager { get; set; } = string.Empty;

          public string Program { get; set; } = string.Empty;

          public List<ProjectsOnlyMetricDto> Metrics { get; set; } = new();

          // ✅ ADD THESE TWO PROPERTIES
          public Dictionary<string, decimal?> OriginalMetrics { get; set; } = new();

          public Dictionary<string, decimal?> RiskAdjustedMetrics { get; set; } = new();
    }
}
