namespace AEPW.Application.DTOs.Report.ProjectReportCpm
{
  public class ProjectReportCpmReportDto
  {

    public string ReportType { get; set; } = string.Empty;


    public string ReportName { get; set; } = string.Empty;

    public ProjectReportCpmFiltersDto Filters { get; set; } = new();

    public List<ProjectReportCpmProjectDto> Projects { get; set; } = [];

    public ReportGenerationDataDto? GenerationData { get; set; }

  }
  //CPM helper Model for Mapping

  public sealed class ProjectRow
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
  }

  public sealed class TopicRow
  {
    public long ImpactMasterId { get; set; }
    public long CategoryId { get; set; }
    public string TopicCode { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public int CategoryDisplayOrder { get; set; }
  }

  public sealed class MetricRow
  {
    public long ImpactMasterId { get; set; }

    public long CategoryId { get; set; }

    public string TopicCode { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;

    public int CategoryDisplayOrder { get; set; }

    public long TopLevelMetricId { get; set; }
    public string TopLevelMetricCode { get; set; } = string.Empty;
    public string TopLevelMetricName { get; set; } = string.Empty;

    public long? L1MetricId { get; set; }
    public string? L1MetricCode { get; set; }
    public string? L1MetricName { get; set; }

    public long MetricId { get; set; }
    public string MetricLevel { get; set; } = string.Empty;

    public int Year { get; set; }

    public decimal? Q1ReportedData { get; set; }
    public decimal? Q2ReportedData { get; set; }
    public decimal? Q3ReportedData { get; set; }
    public decimal? Q4ReportedData { get; set; }
    public decimal? TotalReportedData { get; set; }

    public decimal? Q1RiskAdjustedData { get; set; }
    public decimal? Q2RiskAdjustedData { get; set; }
    public decimal? Q3RiskAdjustedData { get; set; }
    public decimal? Q4RiskAdjustedData { get; set; }
    public decimal? TotalRiskAdjustedData { get; set; }
  }


}
