namespace AEPW.Application.DTOs.Report.Summary
{
  public class SummaryReportRequestDto
  {
    public List<string>? ProjectIds { get; set; }

    public List<string>? ProjectNameIds { get; set; }

    public List<string>? CpmIds { get; set; }

    public List<string>? ProgramLeaderIds { get; set; }

    public List<string>? GlobalExecutionLeaderIds { get; set; }

    //public List<string>? FundingRecipientIds { get; set; }

    public List<string>? ProgramIds { get; set; }

    public List<string>? CountryCityIds { get; set; }

    public List<string>? ProjectStatusIds { get; set; }
    public List<string> L1MetricIds { get; set; } = [];

    public List<string> ProjectCategoryIds { get; set; } = [];

    public List<string> SourceTypeIds { get; set; } = [];

    public List<string>? RecordStatusIds { get; set; } = [];

    public string UserId { get; set; } = string.Empty;
    public DateTime GenerationDate { get; set; }
  }
}
