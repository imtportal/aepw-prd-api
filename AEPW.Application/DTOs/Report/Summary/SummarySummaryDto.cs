namespace AEPW.Application.DTOs.Report.Summary
{
    public class SummarySummaryDto
    {
        public decimal? TotalSinceInception { get; set; }

        public List<YearlyValueDto> YearlyRiskAdjustedValues { get; set; }
    }
}
