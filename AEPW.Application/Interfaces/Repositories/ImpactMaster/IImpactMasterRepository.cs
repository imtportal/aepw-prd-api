
using System.Text;
using AEPW.Application.DTOs.ImpactMaster;
using AEPW.Domain.Models.Metric;
using AEPW.Application.Contracts.Responses;

namespace AEPW.Application.Interfaces.Repositories.ImpactMaster
{
    public interface IImpactMasterRepository
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


        Task<IReadOnlyList<ImpactMasterMetricHierarchyModel>> GetMetricsForImpactMasterAsync(
            string? impactMasterId,
            string? recordTypeId,
            CancellationToken cancellationToken = default);

        Task SaveImpactMasterMetricsAsync(
            SaveImpactMasterMetricsRequest request,
            CancellationToken cancellationToken = default);
        Task<DeleteImpactMasterResponseDto> DeleteImpactMasterAsync(
            DeleteImpactMasterRequestDto request,
            CancellationToken cancellationToken = default);

        Task<CopyImpactMasterResponseDto> CopyImpactMasterAsync(
           CopyImpactMasterRequestDto request,
           CancellationToken cancellationToken = default);

        Task<ImpactMasterFilterOptionsDto> ImpactMasterFilterOptions(ImpactMasterFilterRequestDto request,

      CancellationToken cancellationToken = default);

    }
}
