namespace AEPW.Application.DTOs.Report.Summary
{
    public class SummaryMetricSummaryDto
    {
        public decimal? TotalSinceInception { get; set; }

        public List<YearlyValueDto> YearlyRiskAdjustedValues { get; set; }
            = new();
    }
}
