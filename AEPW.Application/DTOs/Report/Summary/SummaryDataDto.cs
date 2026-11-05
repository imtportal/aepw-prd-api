namespace AEPW.Application.DTOs.Report.Summary
{
    public class SummaryDataDto
    {
        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public SummaryMetricDto TopLevelMetric { get; set; }
            = new();

        public decimal? TotalSinceInception { get; set; }

        public List<YearlyValueDto> YearlyRiskAdjustedValues { get; set; }
            = new();

        public string? AggregationType { get; set; }

        public List<SummaryL1MetricDto> L1Metrics { get; set; }
            = new();

        public SummaryMetricSummaryDto? Summary { get; set; } 

      
    }
}
