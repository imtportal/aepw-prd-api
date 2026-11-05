namespace AEPW.Application.DTOs.Report.ProjectReportCpm
{
  public class ProjectReportCpmTopicDto
  {
    public long CategoryId { get; set; }

    public string TopicCode { get; set; } = string.Empty;

    public string TopicName { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public List<ProjectReportCpmMetricDto> TopLevelMetrics { get; set; }
        = [];
  }
  //

}
