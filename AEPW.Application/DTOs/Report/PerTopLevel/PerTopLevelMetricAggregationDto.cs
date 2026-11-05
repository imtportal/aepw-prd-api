namespace AEPW.Application.DTOs.Report.PerTopLevelMetric
{
	public class PerTopLevelMetricAggregationDto
	{
		public string MetricCode { get; set; } = string.Empty;

		public string MetricName { get; set; } = string.Empty;

		public string Unit { get; set; } = string.Empty;

		// ✅ FIX: Use PerTopLevelMetricAggregationYearlyDataDto instead of PerTopLevelMetricYearlyDataDto
		public List<PerTopLevelMetricAggregationYearlyDataDto> YearlyData { get; set; }
			= new();
	}
}
