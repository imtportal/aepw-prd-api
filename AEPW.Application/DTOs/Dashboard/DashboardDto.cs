using System.Collections.Generic;

namespace AEPW.Application.DTOs.Dashboard
{
  public sealed class DashboardPortfolioListDto
  {
    public List<DashboardMetricDto> Metrics { get; set; } = new();

    public List<DashboardProgramDto> Programs { get; set; } = new();

    public int TotalRecord { get; set; }
  }

  public sealed class DashboardMetricDto
  {
    public string? MetricId { get; set; }
    public string? MetricCode { get; set; }
    public string? MetricName { get; set; }
    public string? CategoryCode { get; set; }
    public string? Color { get; set; }
  }

  public sealed class DashboardProgramDto
  {
    public string? ProgramId { get; set; }
    public string? ProgramCode { get; set; }
    public string? ProgramName { get; set; }

    public string? ProjectCount { get; set; }

    public List<DashboardProgramMetricDto> Metrics { get; set; } = new();

    public List<DashboardProgramImpactDto> Impacts { get; set; } = new();
  }

  public sealed class DashboardProgramMetricDto
  {
    public string? MetricId { get; set; }
    public string? MetricCode { get; set; }
    public string? RiskAdjustment { get; set; }
  }

  public sealed class DashboardProgramImpactDto
  {
    public string? ImpactMasterId { get; set; }
    public string? Pin { get; set; }
    public string? ImpactName { get; set; }

    public string? CategoryId { get; set; }
    public string? CategoryCode { get; set; }
    public string? CategoryName { get; set; }
    public string? Color { get; set; }

    public string? StatusId { get; set; }
    public string? StatusCode { get; set; }
    public string? StatusName { get; set; }

    public string? RecordTypeId { get; set; }
    public string? RecordTypeName { get; set; }

    public List<DashboardImpactMetricDto> Metrics { get; set; } = new();

    public string? FundingRecipients { get; set; }
  }

  public sealed class DashboardImpactMetricDto
  {
    public string? MetricId { get; set; }
    public string? MetricCode { get; set; }
    public string? RiskAdjustment { get; set; }
  }

  public sealed class DashboardPortfolioAdjustmentByCategoriesDto
  {
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? MetricId { get; set; }
    public string? RiskAdjustment { get; set; }
    public string? QuarterlyGrowth { get; set; }
    public string? AnnualGrowth { get; set; }
    public string? Color { get; set; }
  }
}
