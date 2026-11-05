using  AEPW.Application.DTOs.Report.Summary;
using  AEPW.Application.DTOs.Report.ReviewMetricTracker;
using  AEPW.Application.DTOs.Report.ProjectReportCpm;
using  AEPW.Application.DTOs.Report.PerTopLevelMetric;
using  AEPW.Application.DTOs.Report.ProjectsOnly;
using AEPW.Application.DTOs.Report.AnnualPortfolio;
using System.Data;
using AEPW.Application.DTOs.Report;
using DocumentFormat.OpenXml.Presentation;

namespace AEPW.Application.Interfaces.Repositories.Report
{
  public interface IReportRepository
  {
    //Task<SummaryReportDto> GetSummaryReportAsync(
    //    SummaryReportRequestDto request);

    Task<DataSet> GetSummaryReportAsync(
         SummaryReportRequestDto request, CancellationToken cancellationToken);


    //Task<ReviewMetricTrackerReportDto>
    //        GetReviewMetricTrackerReportAsync(
    //            ReviewMetricTrackerFiltersDto request, CancellationToken cancellationToken);

    Task<DataSet> GetReviewMetricTrackerReportAsync(
    ReviewMetricTrackerFiltersDto request,
    CancellationToken cancellationToken);

    //Task<ProjectReportCpmReportDto>
    //    GetProjectReportCpmReportAsync(
    //        ProjectReportCpmRequestDto request);

    Task<DataSet> GetProjectReportCpmReportAsync(
    ProjectReportCpmFiltersDto request, CancellationToken cancellationToken);

    //Task<PerTopLevelMetricReportDto>
    //    GetPerTopLevelMetricReportAsync(
    //        PerTopLevelMetricRequestDto request);
    Task<DataSet> GetPerTopLevelMetricReportAsync(
PerTopLevelMetricRequestDto request, CancellationToken cancellationToken);

    //    Task<AnnualPortfolioRiskAdjustedReportDto>
    //GetAnnualPortfolioRiskAdjustedReportAsync(
    //    AnnualPortfolioRequestDto request);

    Task<DataSet> GetAnnualPortfolioRiskAdjustedReportAsync(
    AnnualPortfolioRequestDto request , CancellationToken cancellationToken);


    //Task<ProjectsOnlyReportDto>
    //    GetProjectsOnlyReportAsync(
    //        ProjectsOnlyRequestDto request);

    Task<DataSet> GetProjectsOnlyReportAsync(
    ProjectsOnlyRequestDto request , CancellationToken cancellationToken);

    // --- below is filters options
    //Task<MetricFilterOptionsDto> ReportMetricFilterOptions(
    //    MetricFilterRequestDto request,
    //    CancellationToken cancellationToken = default);

    Task<DataSet> ReportMetricFilterOptions(
    MetricFilterRequestDto request,
    CancellationToken cancellationToken = default);

    Task<DataSet> ReportFilterProjectCascadingOptions(
    ReportFilterProjectCascadingRequestDto request,
    CancellationToken cancellationToken = default);

    Task<DataSet> ReportIndependentFilterOptions(
    List<string>? projectIds,
    CancellationToken cancellationToken = default);
  }
}
