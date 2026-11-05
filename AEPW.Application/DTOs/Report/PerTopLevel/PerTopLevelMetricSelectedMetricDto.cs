namespace AEPW.Application.Report.PerTopLevelMetric
{
    public class PerTopLevelMetricSelectedMetricDto
    {
        public string MetricCode { get; set; } = string.Empty;

        public string MetricName { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;

        public decimal? TotalRiskAdjusted { get; set; }
    }
}
