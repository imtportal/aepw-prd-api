namespace AEPW.Application.DTOs.Report.ReviewMetricTracker
{
    public class ReviewMetricTrackerMetricDataDto
    {
        public decimal? RawValue { get; set; }

        public decimal? AdjustedValue { get; set; }

        public decimal? RiskOriginal { get; set; }

        public decimal? RiskPctProposed { get; set; }

        public decimal? RiskPctEffective { get; set; }

        public decimal? VarianceAbs { get; set; }

        public decimal? VariancePct { get; set; }

        public string? ValidationStatus { get; set; }

        public int EvidenceCount { get; set; }

        public string? LastValueSource { get; set; }
    }
}
