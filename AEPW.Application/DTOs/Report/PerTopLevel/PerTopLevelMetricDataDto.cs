namespace AEPW.Application.DTOs.Report.PerTopLevelMetric
{
    public class PerTopLevelMetricDataDto
    {
        public string? MetricCode { get; set; }

        public string? MetricName { get; set; }

        public string? Unit { get; set; }

        public decimal? Annual { get; set; }

        public decimal? EvidenceRating { get; set; }

        public decimal? RiskAdjusted { get; set; }

        public decimal? ContributionPercentage { get; set; }

        public decimal? Baseline { get; set; }

        public List<PerTopLevelMetricYearlyDataDto>? YearlyData { get; set; }
    }
}
