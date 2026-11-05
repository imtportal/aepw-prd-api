namespace AEPW.Application.DTOs.Report.AnnualPortfolio
{
    public class AnnualPortfolioMetricCategoryDto
    {
        public string CategoryCode { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string? Color { get; set; }

        public int DisplayOrder { get; set; }

        public List<AnnualPortfolioMetricDto> Metrics { get; set; }
            = new();
    }
}
