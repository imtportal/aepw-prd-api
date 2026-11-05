namespace AEPW.Application.DTOs.Report.ProjectsOnly
{
  public class ProjectsOnlyFiltersDto
  {
    public List<string> ReportingYears { get; set; } = [];

    public List<string> Quarters { get; set; } = [];
    public List<string>? ReviewPeriodCodes { get; set; } = [];

    public List<string>? ProjectIds { get; set; }

    //public List<string>? ProjectNameIds { get; set; }

    public List<string>? CpmIds { get; set; }

    public List<string>? ProgramLeaderIds { get; set; }

    public List<string>? GlobalExecutionLeaderIds { get; set; }

    public List<string>? FundingRecipientIds { get; set; }

    public List<string>? ProgramIds { get; set; }

    public List<string>? CountryCityIds { get; set; }


    public List<string>? ProjectStatusIds { get; set; }

    public List<string>? ProjectCategoryIds { get; set; }
    public List<string> RecordStatusIds { get; set; } = new();

      public string? UserId { get; set; }
  }


}
