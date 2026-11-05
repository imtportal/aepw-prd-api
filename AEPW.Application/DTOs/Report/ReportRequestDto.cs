using System.Text.Json;

namespace AEPW.Application.DTOs.Report
{
  public class ReportRequestDto
  {
    public string ReportType { get; set; } = string.Empty;

    public JsonElement Filters { get; set; }
    public string UserId { get; set; } = string.Empty;
  }

  public sealed class MetricFilterOptionsDto
  {
    public List<MetricOptionsDto> TopicMetricOptions { get; set; } = new();
    public List<MetricOptionsDto> ToplevelMetricOptions { get; set; } = new();
    public List<MetricOptionsDto> L1MetricOptions { get; set; } = new();
    public object Tables { get; internal set; }
  }

  public sealed class MetricOptionsDto
  {
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OptionName { get; set; } = string.Empty;
  }

  public sealed class ReportFilterOptionDto
  {
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
  }

  public sealed class ReportFilterProjectOptionDto
  {
    public string ProjectId { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
  }

  public sealed class ReportFilterProjectCascadingOptionsDto
  {
    public List<ReportFilterProjectOptionDto> ProjectOptions { get; set; } = new();

    public List<ReportFilterOptionDto> CPMOptions { get; set; } = new();
    public List<ReportFilterOptionDto> GELOptions { get; set; } = new();
    public List<ReportFilterOptionDto> PLOptions { get; set; } = new();

    public List<ReportFilterOptionDto> FundingRecipientOptions { get; set; } = new();

    public List<ReportFilterOptionDto> ProgramOptions { get; set; } = new();

    public List<ReportFilterOptionDto> CountryCityOptions { get; set; } = new();

    //public List<ReportFilterOptionDto> CityOptions { get; set; } = new();
  }

  public sealed class ReportFilterIndependentOptionsDto
  {
    public List<ReportFilterOptionDto> ProjectStatusOptions { get; set; } = new();

    public List<ReportFilterOptionDto> SourceTypeOptions { get; set; } = new();
  }

  public sealed class MetricFilterRequestDto
  {
    public List<string>? TopicIds { get; set; }
    public List<string>? TopLevelMetricIds { get; set; }
    public List<string>? L1MetricIds { get; set; }
  }

  public sealed class ReportFilterProjectCascadingRequestDto
  {
    public List<string>? ProjectIds { get; set; }

    public List<string>? CPMUserIds { get; set; }

    public List<string>? GELUserIds { get; set; }

    public List<string>? PLUserIds { get; set; }

    public List<string>? FundingRecipientIds { get; set; }

    public List<string>? ProgramIds { get; set; }

    public List<string>? CountryCodes { get; set; }

    public List<string>? CityIds { get; set; }

    public List<string>? ReportingYears { get; set; }

    public List<string>? QuarterCodes { get; set; }

    public string? SelectedReportType { get; set; }

  }

  public sealed class ReportIndependentFilterOptionsDto
  {
    public List<ReportFilterOptionDto> SourceTypeOptions { get; set; } = new();
    public List<ReportFilterOptionDto> ProjectStatusOptions { get; set; } = new();
    public List<ReportFilterOptionDto> ReportingYearOptions { get; set; } = new();
    public List<ReportFilterOptionDto> ReportingQuarterOptions { get; set; } = new();
    public List<ReportFilterOptionDto> RecordStatusOptions { get; set; } = new();
    public List<ReportFilterOptionDto> ProjectCategoryOptions { get; set; } = new();
  }

  public sealed class ReportGenerationDataDto
  {
    public string GeneratedByUser { get; set; } = string.Empty;
    public DateTime GenerationDate { get; set; }
  }

}
