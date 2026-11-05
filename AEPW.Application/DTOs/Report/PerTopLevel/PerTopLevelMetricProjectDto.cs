namespace AEPW.Application.DTOs.Report.PerTopLevelMetric
{
	public class PerTopLevelMetricProjectDto
	{
		public string ImId { get; set; } = string.Empty;

		public string Pin { get; set; } = string.Empty;

		public string ProjectTitle { get; set; } = string.Empty;


    public List<PerTopLevelMetricDataDto> Metrics { get; set; }
			= new();
	}
}
