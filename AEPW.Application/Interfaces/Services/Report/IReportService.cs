using AEPW.Application.DTOs.Report;
using AEPW.Application.DTOs.Report.Summary;
using AEPW.Application.DTOs.Report.ReviewMetricTracker;
using AEPW.Application.DTOs.Report.ProjectReportCpm;
using AEPW.Application.DTOs.Report.PerTopLevelMetric;
using AEPW.Application.Contracts.Responses;

namespace AEPW.Application.Interfaces.Services.Report
{
  public interface IReportService
  {
    Task<SummaryReportDto> GetSummaryReportAsync(
        SummaryReportRequestDto request, CancellationToken cancellationToken);

    Task<byte[]> GenerateSummaryExcelAsync(
        SummaryReportRequestDto request, CancellationToken cancellationToken);

    Task<byte[]> GenerateReviewMetricTrackerExcelAsync(
        ReviewMetricTrackerFiltersDto request , CancellationToken cancellationToken);

    Task<ReportFileResultDto> GenerateReportAsync(
        ReportRequestDto request, CancellationToken cancellationToken);

    Task<byte[]> GenerateProjectReportCpmExcelAsync(
    ProjectReportCpmFiltersDto request, CancellationToken cancellationToken = default);

    Task<byte[]> GeneratePerTopLevelMetricExcelAsync(
PerTopLevelMetricRequestDto request , CancellationToken cancellationToken);

    //-----------filter options------------------------
    Task<ApiResponse<MetricFilterOptionsDto>> ReportMetricFilterOptions(
   MetricFilterRequestDto request,
   CancellationToken cancellationToken = default);


    Task<ApiResponse<ReportFilterProjectCascadingOptionsDto>> ReportFilterProjectCascadingOptions(
        ReportFilterProjectCascadingRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<ReportIndependentFilterOptionsDto>>
    ReportIndependentFilterOptions(List<string>? projectIds,
        CancellationToken cancellationToken = default);
  }
}
