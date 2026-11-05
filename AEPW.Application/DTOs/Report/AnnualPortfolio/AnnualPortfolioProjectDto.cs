namespace AEPW.Application.DTOs.Report.AnnualPortfolio
{
    public class AnnualPortfolioProjectDto
  {
        public string? ImId { get; set; } // Year + ImID (unique key)
        public string? Year { get; set; } // Reporting Year (names in column)

        public string? Pin { get; set; }

        public string? ProjectTitle { get; set; } // Title

        public string? ProjectManager { get; set; } // Contract Performance Manager 

        public string? FundingRecipient { get; set; } 

        public string? City { get; set; }

        public string? Program { get; set; }

        public string? ProjectStatus { get; set; }
        public string? ProjectCategory { get; set; }

    public Dictionary<string, AnnualPortfolioMetricValueDto> MetricValues { get; set; }
            = new();
    }
}
