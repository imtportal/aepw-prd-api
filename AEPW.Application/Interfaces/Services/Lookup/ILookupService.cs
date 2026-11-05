using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.BusinessRoleMapping;
using AEPW.Application.DTOs.Lookup;

namespace AEPW.Application.Interfaces.Services.Lookup;

public interface ILookupService
{
    Task<ApiResponse<List<AppUserLookupDto>>> GetAllAppUsersAsync(string? searchText, CancellationToken cancellationToken);
    Task<ApiResponse<List<CountryLookupDto>>> GetAllCitiesAsync();
    Task<ApiResponse<List<RecordTypeLookupDto>>> GetAllRecordTypesAsync();
    Task<ApiResponse<List<ProgramLookupDto>>> GetAllProgramsAsync(string? searchTerm);
    Task<ApiResponse<List<StatusLookupDto>>> GetStatusesByModuleAsync(string module, string? pageKey, long? impactMasterId);

    Task<ApiResponse<List<MetricCategoryLookupDto>>> GetAllMetricCategoriesAsync();
    Task<ApiResponse<List<MetricSourceLookupDto>>> GetAllMetricSourcesAsync();
    Task<ApiResponse<List<MetricUnitLookupDto>>> GetAllMetricUnitsAsync();
    Task<ApiResponse<List<ParentMetricLookupDto>>> GetAllParentMetricsAsync();
    Task<ApiResponse<List<TimeZoneLookupDto>>> GetAllTimeZonesAsync(string? searchTerm);
    Task<ApiResponse<List<SystemRoleLookupDto>>> GetAllSystemRolesAsync();
    Task<ApiResponse<List<FundingRecipientLookupDto>>> GetAllFundingRecipientsAsync();

    Task<ApiResponse<List<BusinessRoleLookupDto>>> GetAllBusinessRolesAsync();
    Task<ApiResponse<List<BusinessRoleUserLookupDto>>> GetBusinessRoleUsersAsync(
        string? roleCode,
        long? impactMasterId,
        CancellationToken cancellationToken = default);

    //Task<ApiResponse<SaveBusinessRoleMappingResponseDto>>
    //SaveBusinessRoleMappingAsync(
    //    SaveBusinessRoleMappingRequestDto request);

    //Task<ApiResponse<BusinessRoleMappingDetailDto>>
    //GetBusinessRoleMappingByIdAsync(
    //    string businessRoleMappingId);
    Task<ApiResponse<List<ProjectManagerLookupDto>>> GetProjectManagersAsync(
    string? searchTerm);
    Task<ApiResponse<List<PhaseLookupDto>>> GetPhaseLookupAsync();
    Task<ApiResponse<List<ImpactCategoryLookupDto>>> GetImpactCategoryLookupAsync();
    Task<ApiResponse<List<ImpactMasterLookupDto>>> GetImpactMasterLookupAsync(
    string? searchTerm);
    Task<IReadOnlyList<ReviewStageLookupDto>> GetReviewStages();
    Task<IReadOnlyList<ReviewStageLookupDto>> GetReviewStagesByUserId(long userId);

}
