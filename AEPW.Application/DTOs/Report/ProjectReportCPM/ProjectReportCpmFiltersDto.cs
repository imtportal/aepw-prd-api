namespace AEPW.Application.DTOs.Report.ProjectReportCpm
{
  public class ProjectReportCpmFiltersDto
  {
    public List<string> ReportingYears { get; set; } = [];
    public List<string> Quarters { get; set; } = [];
    public List<string>? ReviewPeriodCodes { get; set; } = [];


    public List<string> TopicIds { get; set; } = [];

    public List<string> TopLevelMetricIds { get; set; } = [];
    public List<string> L1MetricIds { get; set; } = [];

    public List<string>? ProjectIds { get; set; } = [];

    //public List<string>? ProjectNameIds { get; set; } = [];

    public List<string>? CpmIds { get; set; } = [];

    public List<string>? ProgramLeaderIds { get; set; } = [];

    public List<string>? GlobalExecutionLeaderIds { get; set; } = [];

    public List<string>? FundingRecipientIds { get; set; } = [];

    public List<string>? ProgramIds { get; set; } = [];

    public List<string>? CountryCityIds { get; set; } = [];
    public List<string>? ProjectStatusIds { get; set; } = [];

    public List<string> ProjectCategoryIds { get; set; } = [];

    //public List<string>? SourceTypeIds { get; set; } = [];

    public List<string>? RecordStatusIds { get; set; } = [];

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? UserId { get; set; }

    // Display Name to show in report
    public List<string>? Pin { get; set; }
    public List<string>? ProjectTitle { get; set; }
    public List<string>? CPM { get; set; }
    public List<string>? Program { get; set; }
    public List<string>? Topics { get; set; }
  }
}
