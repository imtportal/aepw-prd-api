using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Metric;
using AEPW.Application.DTOs.ImpactMaster;
using AEPW.Application.Interfaces.Repositories;
using AEPW.Application.Interfaces.Repositories.ImpactMaster;
using AEPW.Application.Interfaces.Services.ImpactMaster;
namespace AEPW.Application.Services.ImpactMaster
{
    public sealed class ImpactMasterService : IImpactMasterService
    {
        private readonly IImpactMasterRepository _impactMasterRepository;

        public ImpactMasterService(IImpactMasterRepository impactMasterRepository)
        {
            _impactMasterRepository = impactMasterRepository;
        }

        public async Task<PagedResponse<ImpactMasterListDto>> GetAllImpactMastersAsync(
            GetImpactMastersRequestDto request,
            CancellationToken cancellationToken = default)
        {
            return await _impactMasterRepository.GetAllImpactMastersAsync(
                request,
                cancellationToken);
        }

        public async Task<CreateImpactMasterResponseDto> CreateImpactMasterAsync(
            CreateImpactMasterRequestDto request,
            CancellationToken cancellationToken = default)
        {
            return await _impactMasterRepository.CreateImpactMasterAsync(request, cancellationToken);
        }

        public async Task<ImpactMasterDetailDto?> GetImpactMasterByIdAsync(
            long impactMasterId,
            CancellationToken cancellationToken = default)
        {
            return await _impactMasterRepository.GetImpactMasterByIdAsync(impactMasterId, cancellationToken);
        }

        public async Task<List<long>> GetReviewIds(
            long impactMasterId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            return await _impactMasterRepository.GetReviewIds(
                impactMasterId,
                startDate,
                endDate,
                cancellationToken);
        }

        public async Task<bool> UpdateImpactMasterByIdAsync(
    UpdateImpactMasterRequestDto request,
    CancellationToken cancellationToken = default)
        {
            return await _impactMasterRepository.UpdateImpactMasterByIdAsync(
                request,
                cancellationToken);
        }


        public async Task<ApiResponse<List<MetricCategoryDto>>> GetMetricsForImpactMasterAsync(
            string? impactMasterId,
            string? recordTypeId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Domain.Models.Metric.ImpactMasterMetricHierarchyModel> rows =
                await _impactMasterRepository.GetMetricsForImpactMasterAsync(
                    impactMasterId,
                    recordTypeId,
                    cancellationToken);

            Dictionary<string, MetricCategoryDto> categories = [];
            Dictionary<string, MetricDto> metrics = [];

            // First pass: create categories and all metric objects.
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

                        ImpactMasterMetricId = row.ImpactMasterMetricId,
                        IsRiskAdjustmentRequired = row.IsRiskAdjustmentRequired
                    };
                }
            }

            // Second pass: build the hierarchy.
            foreach (var row in rows)
            {
                MetricDto metric = metrics[row.MetricId];

                if (string.IsNullOrWhiteSpace(row.ParentMetricId))
                {
                    categories[row.CategoryId].Metrics.Add(metric);
                }
                else if (metrics.TryGetValue(row.ParentMetricId, out MetricDto? parentMetric))
                {
                    parentMetric.Children.Add(metric);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Parent metric '{row.ParentMetricId}' was not returned by " +
                        $"uspImpactMasterGetMetricsForImpactMaster for child metric " +
                        $"'{row.MetricId}' ({row.MetricCode}).");
                }
            }

            return ApiResponse<List<MetricCategoryDto>>.Ok(
                categories.Values.ToList());
        }


        public async Task<ApiResponse<bool>> SaveImpactMasterMetricsAsync(
            SaveImpactMasterMetricsRequest request)
        {
            await _impactMasterRepository.SaveImpactMasterMetricsAsync(request);

            return ApiResponse<bool>.Ok(true, "Impact master metrics saved successfully.");
        }
        public async Task<DeleteImpactMasterResponseDto> DeleteImpactMasterAsync(
DeleteImpactMasterRequestDto request,
CancellationToken cancellationToken = default)
        {
            return await _impactMasterRepository.DeleteImpactMasterAsync(
                request,
                cancellationToken);
        }

        public async Task<CopyImpactMasterResponseDto> CopyImpactMasterAsync(
        CopyImpactMasterRequestDto request,
        CancellationToken cancellationToken = default)
        {
            return await _impactMasterRepository.CopyImpactMasterAsync(
                request,
                cancellationToken);
        }
        public async Task<ApiResponse<ImpactMasterFilterOptionsDto>> ImpactMasterFilterOptions(

    ImpactMasterFilterRequestDto request,

    CancellationToken cancellationToken = default)

        {

            ImpactMasterFilterOptionsDto result =

                await _impactMasterRepository.ImpactMasterFilterOptions(

                    request,

                    cancellationToken);


            var response = new ImpactMasterFilterOptionsDto
            {

                ProgramOptions = result.ProgramOptions

                    .Select(x => new ProgramFilterOptions
                    {

                        ProgramId = x.ProgramId,

                        ProgramCode = x.ProgramCode,

                        ProgramName = x.ProgramName

                    })

                    .ToList(),


                RecordTypeOptions = result.RecordTypeOptions

                    .Select(x => new RecordTypeFilterOptions
                    {

                        RecordTypeId = x.RecordTypeId,

                        RecordTypeName = x.RecordTypeName

                    })

                    .ToList(),


                StatusOptions = result.StatusOptions

                    .Select(x => new StatusFilterOptions
                    {

                        StatusId = x.StatusId,

                        StatusCode = x.StatusCode,

                        StatusName = x.StatusName

                    })

                    .ToList()

            };


            return ApiResponse<ImpactMasterFilterOptionsDto>.Ok(response);

        }
    }
}
