using System;
using System.Collections.Generic;
using System.Text;
using AEPW.Application.DTOs.Metric;
using AEPW.Domain.Models.Metric;
using AEPW.Application.Interfaces.Repositories.Metric;
using AEPW.Application.Interfaces.Services.Metric;
using AEPW.Application.Contracts.Responses;
namespace AEPW.Application.Services.Metric
{
    public sealed class MetricService : IMetricService
    {
        private readonly IMetricRepository _metricRepository;

        public MetricService(IMetricRepository metricRepository)
        {
            _metricRepository = metricRepository;
        }

        public async Task<ApiResponse<List<MetricCategoryDto>>> GetMetricHierarchyAsync()
        {
            MetricHierarchyResultModel result =
          await _metricRepository.GetMetricHierarchyAsync();

            var rows = result.Metrics;
            Dictionary<string, MetricCategoryDto> categories = [];

            Dictionary<string, MetricDto> metrics = [];

            foreach (var row in rows)
            {
                if (!categories.ContainsKey(row.CategoryId))
                {
                    categories[row.CategoryId] = new MetricCategoryDto
                    {
                        CategoryId = row.CategoryId,
                        CategoryCode = row.CategoryCode,
                        CategoryName = row.CategoryName,
                        CategoryColor = row.CategoryColor,
                        CategoryDescription = row.CategoryDescription,
                        L4ApproverUserId = row.L4ApproverUserId,
                        L4ApproverName = row.L4ApproverName,
                    };
                }

                if (!metrics.ContainsKey(row.MetricId))
                {
                    metrics[row.MetricId] = new MetricDto
                    {
                        MetricId = row.MetricId,
                        MetricCode = row.MetricCode,
                        MetricName = row.MetricName,
                        Description = row.Description,
                        MetricHeader = row.MetricHeader,

                        IsTopLevel = row.IsTopLevel,

                        ParentMetricId = row.ParentMetricId,
                        ParentMetricName = row.ParentMetricName,

                        MetricSourceId = row.MetricSourceId,
                        MetricSourceCode = row.MetricSourceCode,
                        MetricSourceName = row.MetricSourceName,

                        IsRiskAdjustmentRequired = row.IsRiskAdjustmentRequired,
                        IsICPRequired = row.IsICPRequired,

                        ICPApproverUserId = row.ICPApproverUserId,
                        ICPApproverName = row.ICPApproverName,
                        IsActive = row.IsActive,

                        MetricUnitId = row.MetricUnitId,
                        UnitName = row.UnitName
                    };
                }
            }

            foreach (var row in rows)
            {
                MetricDto metric = metrics[row.MetricId];

                if (string.IsNullOrWhiteSpace(row.ParentMetricId))
                {
                    categories[row.CategoryId]
                        .Metrics
                        .Add(metric);
                }
                else
                {
                    metrics[row.ParentMetricId]
                        .Children
                        .Add(metric);
                }
            }

            foreach (var evidence in result.EvidenceTypes)
            {
                if (metrics.TryGetValue(evidence.MetricId, out var metric))
                {
                    metric.EvidenceTypes.Add(
                        new MetricEvidenceTypeDto
                        {
                            EvidenceTypeId = evidence.EvidenceTypeId,
                            Code = evidence.Code,
                            Description = evidence.Description,
                            IsRequired = evidence.IsRequired
                        });
                }
            }

            return ApiResponse<List<MetricCategoryDto>>.Ok(
                categories.Values.ToList()
);
        }
        //SAVE

        public async Task<ApiResponse<bool>> SaveMetricConfigurationAsync(
    MetricSaveConfigurationDto request)
        {
            var model = new MetricSaveConfigurationModel
            {
                UserId = request.UserId,
                MetricCategories = request.MetricCategories
                    .Select(x => new MetricCategoryConfigurationModel
                    {
                        CategoryId = long.Parse(x.CategoryId),
                        CategoryDescription = x.CategoryDescription,
                        L4ApproverUserId = string.IsNullOrWhiteSpace(x.L4ApproverUserId)
                            ? null
                            : long.Parse(x.L4ApproverUserId)
                    })
                    .ToList(),

                Metrics = request.Metrics
                    .Select(x => new MetricConfigurationModel
                    {
                        MetricId = long.Parse(x.MetricId),
                        Description = x.Description,
                        MetricHeader = x.MetricHeader,

                        MetricSourceId = string.IsNullOrWhiteSpace(x.MetricSourceId)
                            ? null
                            : long.Parse(x.MetricSourceId),

                        ICPApproverUserId = string.IsNullOrWhiteSpace(x.ICPApproverUserId)
                            ? null
                            : long.Parse(x.ICPApproverUserId),

                        IsICPRequired = x.IsICPRequired,
                        IsRiskAdjustmentRequired = x.IsRiskAdjustmentRequired,
                        IsActive = x.IsActive
                    })
                    .ToList()
            };

            await _metricRepository.SaveMetricConfigurationAsync(model);

            return ApiResponse<bool>.Ok(true);
        }



        public async Task<ApiResponse<List<MetricDto>>> GetAllMetricsAsync()
        {
            IReadOnlyList<MetricDto> rows =
            await _metricRepository.GetAllMetricsAsync();
            List<MetricDto> metrics = [];

            foreach (var row in rows)
            {
                metrics.Add(
                    new MetricDto()
                    {
                        MetricId = row.MetricId,

                        CategoryId = row.CategoryId,

                        MetricCode = row.MetricCode,

                        MetricName = row.MetricName,

                        Description = row.Description,

                        IsTopLevel = row.IsTopLevel,

                        ParentMetricId = row.ParentMetricId,

                        ParentMetricName = row.ParentMetricName,

                        MetricSourceId = row.MetricSourceId,

                        MetricSourceCode = row.MetricSourceCode,

                        MetricSourceName = row.MetricSourceName,



                        IsRiskAdjustmentRequired = row.IsRiskAdjustmentRequired,

                        IsICPRequired = row.IsICPRequired,

                        ICPApproverUserId = row.ICPApproverUserId,

                        ICPApproverName = row.ICPApproverName,



                        IsActive = row.IsActive,



                        MetricUnitId = row.MetricUnitId,

                        UnitName = row.UnitName,

                        ImpactMasterMetricId = row.ImpactMasterMetricId,

                        Children = row.Children,
                    }
                );
            }

            return ApiResponse<List<MetricDto>>.Ok(
                metrics
            );
        }

        public async Task<ApiResponse<IReadOnlyList<MetricDto>>> GetMetricsForProject(int ProjectId)
        {
            IReadOnlyList<MetricDto> rows = await _metricRepository.GetMetricsForProject(ProjectId);


            return ApiResponse<IReadOnlyList<MetricDto>>.Ok(
                rows
            );
        }

        public async Task<ApiResponse<List<MetricCategoryDto>>> GetAssignableMetricsForReview(long reviewId)
        {
            IReadOnlyList<Domain.Models.Metric.ImpactMasterMetricHierarchyModel> rows = await _metricRepository.GetAssignableMetricsForReview(reviewId);


            Dictionary<string, MetricCategoryDto> categories = [];
            Dictionary<string, MetricDto> metrics = [];

            foreach (var row in rows)
            {
                if (!categories.ContainsKey(row.CategoryId))
                {
                    categories[row.CategoryId] = new MetricCategoryDto
                    {
                        CategoryId = row.CategoryId,
                        CategoryCode = row.CategoryCode,
                        CategoryName = row.CategoryName,
                        CategoryColor = row.CategoryColor
                    };
                }

                if (!metrics.ContainsKey(row.MetricId))
                {
                    metrics[row.MetricId] = new MetricDto
                    {
                        MetricId = row.MetricId,
                        MetricCode = row.MetricCode,
                        MetricName = row.MetricName,
                        Description = row.Description,

                        IsTopLevel = row.IsTopLevel,

                        ParentMetricId = row.ParentMetricId,
                        ParentMetricName = row.ParentMetricName,

                        IsActive = row.IsActive,
                        MetricUnitId = row.MetricUnitId,
                        UnitName = row.UnitName,
                        RiskPct = row.RiskPct,


                        BaselinePct = row.BaselinePct,
                        //AverageRiskPct = row.AverageRiskPct,

                        ImpactMasterMetricId = row.ImpactMasterMetricId,

                        IsRiskAdjustmentRequired = row.IsRiskAdjustmentRequired
                    };
                }
            }

            foreach (var row in rows)
            {
                MetricDto metric = metrics[row.MetricId];

                if (string.IsNullOrWhiteSpace(row.ParentMetricId))
                {
                    categories[row.CategoryId]
                        .Metrics
                        .Add(metric);
                }
                else
                {
                    try
                    {
                        metrics[row.ParentMetricId]
                            .Children
                            .Add(metric);

                    }
                    catch
                    {

                    }
                }
            }

            return ApiResponse<List<MetricCategoryDto>>.Ok(
                categories.Values.ToList());


            //return ApiResponse<IReadOnlyList<MetricDto>>.Ok(
            //    rows
            //);
        }

        public async Task<ApiResponse<IReadOnlyList<MetricDto>>> GetMetricsForReview(int ReviewId)
        {
            IReadOnlyList<MetricDto> rows = await _metricRepository.GetMetricsForReview(ReviewId);


            return ApiResponse<IReadOnlyList<MetricDto>>.Ok(
                rows
            );
        }

        public async Task<List<MetricEvidenceTypeDto>> GetEvidenceTypeForMetric(string metricCodes, string metricIds)
        {
            return await _metricRepository.GetEvidenceTypeForMetric(metricCodes, metricIds);
        }
    }
}
