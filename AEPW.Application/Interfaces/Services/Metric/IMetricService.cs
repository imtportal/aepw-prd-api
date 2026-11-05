using System;
using System.Collections.Generic;
using System.Text;
using AEPW.Application.DTOs.Metric;
using AEPW.Application.Contracts.Responses;
namespace AEPW.Application.Interfaces.Services.Metric
{
    public interface IMetricService
    {
        Task<ApiResponse<List<MetricCategoryDto>>> GetMetricHierarchyAsync();
        Task<ApiResponse<bool>> SaveMetricConfigurationAsync(
    MetricSaveConfigurationDto request);
        Task<ApiResponse<List<MetricDto>>> GetAllMetricsAsync();
        Task<ApiResponse<IReadOnlyList<MetricDto>>> GetMetricsForProject(int ProjectId);
        Task<ApiResponse<List<MetricCategoryDto>>> GetAssignableMetricsForReview(long reviewId);
        Task<ApiResponse<IReadOnlyList<MetricDto>>> GetMetricsForReview(int ReviewId);
        Task<List<MetricEvidenceTypeDto>> GetEvidenceTypeForMetric(string metricCodes, string metricIds);

    }
}
