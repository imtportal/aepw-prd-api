namespace AEPW.Application.DTOs.Report.PerTopLevelMetric
{
    public class PerTopLevelMetricAggregationYearlyDataDto
    {
        public int Year { get; set; }

        public decimal? Annual { get; set; }  // ✅ ADD THIS

        public decimal? RiskAdjusted { get; set; }  // ✅ ADD THIS

        public PerTopLevelMetricQuarterDataDto? OriginalData { get; set; }

        public PerTopLevelMetricQuarterDataDto? RiskAdjustedData { get; set; }

        public decimal? Baseline { get; set; }
    }
}
