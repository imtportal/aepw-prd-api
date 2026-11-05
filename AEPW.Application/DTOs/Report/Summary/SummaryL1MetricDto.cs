

namespace AEPW.Application.DTOs.Report.Summary
{
    public class SummaryL1MetricDto
    {
        public string MetricCode { get; set; } = string.Empty;

        public string MetricName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal? TotalSinceInception { get; set; }

        public List<YearlyValueDto> YearlyRiskAdjustedValues { get; set; }
            = new();
    }
}
