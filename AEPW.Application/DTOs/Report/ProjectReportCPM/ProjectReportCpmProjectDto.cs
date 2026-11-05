namespace AEPW.Application.DTOs.Report.ProjectReportCpm
{
	public class ProjectReportCpmProjectDto
	{
    public long ImpactMasterId { get; set; }

    public string PIN { get; set; } = string.Empty;

    public string ProjectTitle { get; set; } = string.Empty;

    public string ContractPerformanceManager { get; set; } = string.Empty;

    public string ProgramLeader { get; set; } = string.Empty;

    public string GlobalExecutionLeader { get; set; } = string.Empty;

    public string FundingRecipient { get; set; } = string.Empty;

    public string Program { get; set; } = string.Empty;

    public string CountryCity { get; set; } = string.Empty;

    public List<ProjectReportCpmTopicDto> Topics { get; set; } = [];
  }
}
