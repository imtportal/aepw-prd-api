namespace AEPW.Application.DTOs.Report.ProjectsOnly
{
  public class ProjectsOnlyReportDto
  {
    public string ReportType { get; set; } = string.Empty;

    public string ReportName { get; set; } = string.Empty;

    public ProjectsOnlyFiltersDto Filters { get; set; } = new();

    public List<string> MetricCodes { get; set; } = new();

    public List<ProjectsOnlyProjectDto> Projects { get; set; } = new();

    public ReportGenerationDataDto? ReportGenerationData { get; set; }
  }
}
