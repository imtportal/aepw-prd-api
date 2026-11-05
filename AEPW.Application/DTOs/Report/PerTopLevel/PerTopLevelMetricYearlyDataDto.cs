//using AEPW.ApplicationDTOs.Report.PerTopLevelMetric;

namespace AEPW.Application.DTOs.Report.PerTopLevelMetric
{
	public class PerTopLevelMetricYearlyDataDto
	{
		public int Year { get; set; }

    public decimal? EvidenceRating { get; set; }

    public decimal? ContributionPercentage { get; set; }

    public PerTopLevelMetricQuarterDataDto OriginalData { get; set; }
			= new();

		public PerTopLevelMetricQuarterDataDto RiskAdjustedData { get; set; }
			= new();

		public decimal? Baseline { get; set; }
	}
}
