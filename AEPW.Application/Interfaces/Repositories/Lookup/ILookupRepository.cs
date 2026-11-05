using AEPW.Application.DTOs.Lookup;
using AEPW.Domain.Models.BusinessRoleMapping;
using AEPW.Domain.Models.Lookup;

namespace AEPW.Application.Interfaces.Repositories.Lookup;

public interface ILookupRepository
{
    Task<IReadOnlyList<AppUserLookupModel>> GetAllAppUsersAsync(string? searchText,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CityLookupModel>> GetAllCitiesAsync(
      CancellationToken cancellationToken = default);
    Task<List<RecordTypeLookupModel>> GetAllRecordTypesAsync();

    Task<IReadOnlyList<ProgramLookupModel>> GetAllProgramsAsync(string? searchTerm,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MetricSourceLookupModel>> GetAllMetricSourcesAsync(
    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StatusLookupModel>> GetStatusesByModuleAsync(
        string module,
        string? pageKey,
        long? impactMasterId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MetricCategoryLookupModel>> GetAllMetricCategoriesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MetricUnitLookupModel>> GetAllMetricUnitsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ParentMetricLookupModel>> GetAllParentMetricsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeZoneLookupModel>> GetAllTimeZonesAsync(string? searchTerm,
    CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FundingRecipientLookupModel>> GetAllFundingRecipientsAsync(
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemRoleLookupModel>> GetAllSystemRolesAsync(
    CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BusinessRoleLookupModel>> GetAllBusinessRolesAsync(
CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BusinessRoleUserLookupModel>> GetBusinessRoleUsersAsync(
        string? roleCode,
        long? impactMasterId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ImpactCategoryLookupModel>> GetImpactCategoryLookupAsync(
    CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PhaseLookupModel>> GetPhaseLookupAsync(
    CancellationToken cancellationToken = default);


    Task<IReadOnlyList<ProjectManagerLookupModel>> GetProjectManagersAsync(
        string? searchTerm,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ImpactMasterLookupModel>> GetImpactMasterLookupAsync(
    string? searchTerm,
    CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReviewStageLookupDto>> GetReviewStages(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ReviewStageLookupDto>> GetReviewStagesByUserId(long userId, CancellationToken cancellationToken = default);
}
