using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Metric;
using AEPW.Application.DTOs.ImpactMaster;

namespace AEPW.Application.Interfaces.Services.ImpactMaster
{
    public interface IImpactMasterService
    {
        Task<PagedResponse<ImpactMasterListDto>> GetAllImpactMastersAsync(
            GetImpactMastersRequestDto request,
            CancellationToken cancellationToken = default);

        Task<CreateImpactMasterResponseDto> CreateImpactMasterAsync(
            CreateImpactMasterRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ImpactMasterDetailDto?> GetImpactMasterByIdAsync(
            long impactMasterId,
            CancellationToken cancellationToken = default);

        Task<List<long>> GetReviewIds(
            long impactMasterId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateImpactMasterByIdAsync(
    UpdateImpactMasterRequestDto request,
    CancellationToken cancellationToken = default);

        Task<ApiResponse<List<MetricCategoryDto>>> GetMetricsForImpactMasterAsync(
            string? impactMasterId,
            string? recordTypeId,
            CancellationToken cancellationToken = default);

        Task<ApiResponse<bool>> SaveImpactMasterMetricsAsync(
            SaveImpactMasterMetricsRequest request);
        Task<DeleteImpactMasterResponseDto> DeleteImpactMasterAsync(
   DeleteImpactMasterRequestDto request,
   CancellationToken cancellationToken = default);

        Task<CopyImpactMasterResponseDto> CopyImpactMasterAsync(
             CopyImpactMasterRequestDto request,
             CancellationToken cancellationToken = default);

        Task<ApiResponse<ImpactMasterFilterOptionsDto>> ImpactMasterFilterOptions(

       ImpactMasterFilterRequestDto request,

       CancellationToken cancellationToken = default);
    }
}
