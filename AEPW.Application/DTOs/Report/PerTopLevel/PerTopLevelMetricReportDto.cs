using AEPW.Application.Report.PerTopLevelMetric;

namespace AEPW.Application.DTOs.Report.PerTopLevelMetric
{
    public class PerTopLevelMetricReportDto
    {
        public string ReportType { get; set; } = string.Empty;

        public string ReportName { get; set; } = string.Empty;

        public PerTopLevelMetricFiltersDto Filters { get; set; } = new();

        public List<PerTopLevelMetricSelectedMetricDto> SelectedMetrics { get; set; }
            = new();

        public List<PerTopLevelMetricProjectDto> Projects { get; set; }
            = new();

      public List<PerTopLevelMetricAggregationDto> Aggregations { get; set; }
          = new();

      public ReportGenerationDataDto? GenerationData { get; set; }
  }
}
