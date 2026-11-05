namespace AEPW.Application.DTOs.Report.AnnualPortfolio
{
  public class AnnualPortfolioRequestDto
  {
    public List<string> ReportingYears { get; set; } = new();

    //public List<string> Quarters { get; set; } = new();

    public List<string>? ProjectIds { get; set; } = [];

    public List<string>? ProjectNameIds { get; set; } = [];

    public List<string>? CpmIds { get; set; } = [];

    public List<string>? ProgramLeaderIds { get; set; } = [];

    public List<string>? GlobalExecutionLeaderIds { get; set; } = [];

    public List<string>? FundingRecipientIds { get; set; } = [];

    public List<string>? ProgramIds { get; set; } = [];

    public List<string>? CountryCityIds { get; set; } = [];

    public List<string>? ProjectStatusIds { get; set; } = [];

    public List<string>? SourceTypeIds { get; set; } = [];
    public List<string> ProjectCategoryIds { get; set; } = [];
    public List<string>? RecordStatusIds { get; set; } = [];

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string UserId { get; set; } = string.Empty;
  }

  }
