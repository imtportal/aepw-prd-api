using System;
using System.Collections.Generic;
using System.Text;
using AEPW.Domain.Models.Metric;
using AEPW.Application.DTOs.Metric; 
namespace AEPW.Application.Interfaces.Repositories.Metric
{
    public interface IMetricRepository
    {
        Task<MetricHierarchyResultModel>
              GetMetricHierarchyAsync(
                  CancellationToken cancellationToken = default);
        //save
        Task SaveMetricConfigurationAsync(
    MetricSaveConfigurationModel model,
    CancellationToken cancellationToken = default);

        Task<IReadOnlyList<MetricDto>>
      GetAllMetricsAsync(
          CancellationToken cancellationToken = default);


        Task<IReadOnlyList<MetricDto>>
      GetMetricsForProject(
                  int ProjectId,
                  CancellationToken cancellationToken = default);
      
    Task<IReadOnlyList<ImpactMasterMetricHierarchyModel>>
        GetAssignableMetricsForReview(
                  long reviewId,
                  CancellationToken cancellationToken = default);

      Task<IReadOnlyList<MetricDto>>
      GetMetricsForReview(
                  int ReviewId,
                  CancellationToken cancellationToken = default);

        Task<List<MetricEvidenceTypeDto>> GetEvidenceTypeForMetric(string metricCodes, string metricIds);
    }
}
