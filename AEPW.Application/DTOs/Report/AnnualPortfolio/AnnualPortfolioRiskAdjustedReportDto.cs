namespace AEPW.Application.DTOs.Report.AnnualPortfolio
{
    public class AnnualPortfolioRiskAdjustedReportDto
    {
    public ReportGenerationDataDto GenerationDataDto = new ReportGenerationDataDto();
        public List<AnnualPortfolioMetricCategoryDto> MetricCategories { get; set; }
            = new();

        public List<AnnualPortfolioProjectDto> Projects { get; set; }
            = new();

    public Dictionary<string, AnnualPortfolioMetricValueDto> Totals { get; set; } = new();
  }
}
