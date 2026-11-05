using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.BusinessRoleMapping;
using AEPW.Application.DTOs.Lookup;
using AEPW.Application.Interfaces.Repositories.Lookup;
using AEPW.Application.Interfaces.Repositories.Review;
using AEPW.Application.Interfaces.Services.Lookup;
using AEPW.Domain.Models.Lookup;
namespace AEPW.Application.Services.Lookup;

public sealed class LookupService : ILookupService
{
    private readonly ILookupRepository _lookupRepository;

    public LookupService(ILookupRepository lookupRepository)
    {
        _lookupRepository = lookupRepository;
    }

    public async Task<ApiResponse<List<AppUserLookupDto>>> GetAllAppUsersAsync(string? searchText,
        CancellationToken cancellationToken)
    {
        var rows = await _lookupRepository.GetAllAppUsersAsync(searchText, cancellationToken);

        var data = rows
            .Select(r => new AppUserLookupDto
            {
                UserId = r.UserId,
                DisplayName = r.DisplayName
            })
            .ToList();

        return ApiResponse<List<AppUserLookupDto>>.Ok(data);
    }

    public async Task<ApiResponse<List<CountryLookupDto>>> GetAllCitiesAsync()
    {
        var rows = await _lookupRepository.GetAllCitiesAsync();

        var data = rows
            .GroupBy(r => r.CountryName)
            .Select(g => new CountryLookupDto
            {
                CountryName = g.Key,
                Cities = g.Select(c => new CityLookupDto
                {
                    CityId = c.CityId,
                    CityName = c.CityName
                }).ToList()
            })
            .ToList();

        return ApiResponse<List<CountryLookupDto>>.Ok(data);
    }
    public async Task<ApiResponse<List<RecordTypeLookupDto>>> GetAllRecordTypesAsync()
    {
        var rows = await _lookupRepository.GetAllRecordTypesAsync();

        var data = rows
            .Select(r => new RecordTypeLookupDto
            {
                RecordTypeId = r.RecordTypeId,
                Name = r.Name
            })
            .ToList();

        return ApiResponse<List<RecordTypeLookupDto>>.Ok(data);
    }
    public async Task<ApiResponse<List<ProgramLookupDto>>> GetAllProgramsAsync(string? searchTerm)
    {
        var rows = await _lookupRepository.GetAllProgramsAsync(searchTerm);

        var data = rows
            .Select(r => new ProgramLookupDto
            {
                ProgramId = r.ProgramId,
                Code = r.Code,
                Name = r.Name
            })
            .ToList();

        return ApiResponse<List<ProgramLookupDto>>.Ok(data);
    }
    public async Task<ApiResponse<List<MetricSourceLookupDto>>> GetAllMetricSourcesAsync()
    {
        var rows = await _lookupRepository.GetAllMetricSourcesAsync();

        var data = rows
            .Select(r => new MetricSourceLookupDto
            {
                MetricSourceId = r.MetricSourceId,
                Code = r.Code,
                Name = r.Name
            })
            .ToList();

        return ApiResponse<List<MetricSourceLookupDto>>.Ok(data);
    }


    public async Task<ApiResponse<List<StatusLookupDto>>> GetStatusesByModuleAsync(
        string module,
        string? pageKey,
        long? impactMasterId)
    {
        var rows = await _lookupRepository.GetStatusesByModuleAsync(
            module,
            pageKey,
            impactMasterId);

        var data = rows
            .Select(r => new StatusLookupDto
            {
                StatusId = r.StatusId,
                StatusCode = r.StatusCode,
                StatusName = r.StatusName,
                DisplayOrder = r.DisplayOrder,
                IsDefault = r.IsDefault
            })
            .ToList();

        return ApiResponse<List<StatusLookupDto>>.Ok(data);
    }
    public async Task<ApiResponse<List<MetricCategoryLookupDto>>> GetAllMetricCategoriesAsync()
    {
        var rows = await _lookupRepository.GetAllMetricCategoriesAsync();

        var data = rows
            .Select(r => new MetricCategoryLookupDto
            {
                CategoryId = r.CategoryId,
                CategoryCode = r.CategoryCode,
                Name = r.Name,
                Color = r.Color
            })
            .ToList();

        return ApiResponse<List<MetricCategoryLookupDto>>.Ok(data);
    }

    public async Task<ApiResponse<List<MetricUnitLookupDto>>> GetAllMetricUnitsAsync()
    {
        var rows = await _lookupRepository.GetAllMetricUnitsAsync();

        var data = rows
            .Select(r => new MetricUnitLookupDto
            {
                MetricUnitId = r.MetricUnitId,
                UnitCode = r.UnitCode,
                UnitName = r.UnitName
            })
            .ToList();

        return ApiResponse<List<MetricUnitLookupDto>>.Ok(data);
    }

    public async Task<ApiResponse<List<ParentMetricLookupDto>>> GetAllParentMetricsAsync()
    {
        var rows = await _lookupRepository.GetAllParentMetricsAsync();

        var data = rows
            .Select(r => new ParentMetricLookupDto
            {
                MetricId = r.MetricId,
                Code = r.Code,
                Name = r.Name,
                Description = r.Description,
                CategoryId = r.CategoryId,
            })
            .ToList();

        return ApiResponse<List<ParentMetricLookupDto>>.Ok(data);
    }


    public async Task<ApiResponse<List<TimeZoneLookupDto>>> GetAllTimeZonesAsync(string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm) && searchTerm.Trim().Length < 3)
        {
            return ApiResponse<List<TimeZoneLookupDto>>.Ok([]);
        }
        var rows = await _lookupRepository.GetAllTimeZonesAsync(searchTerm);

        var data = rows
            .Select(t => new TimeZoneLookupDto
            {
                TimeZoneId = t.TimeZoneId,
                TimeZoneCode = t.TimeZoneCode,
                TimeZoneName = t.TimeZoneName
            })
            .ToList();

        return ApiResponse<List<TimeZoneLookupDto>>.Ok(data);
    }
    public async Task<ApiResponse<List<FundingRecipientLookupDto>>> GetAllFundingRecipientsAsync()
    {
        var rows = await _lookupRepository.GetAllFundingRecipientsAsync();

        var data = rows
            .Select(r => new FundingRecipientLookupDto
            {
                FundingRecipientId = r.FundingRecipientId,
                Name = r.Name,
                OrgName = r.OrgName
            })
            .ToList();

        return ApiResponse<List<FundingRecipientLookupDto>>.Ok(data);
    }
    public async Task<ApiResponse<List<SystemRoleLookupDto>>> GetAllSystemRolesAsync()
    {
        var rows = await _lookupRepository.GetAllSystemRolesAsync();

        var data = rows
            .Select(r => new SystemRoleLookupDto
            {
                RoleId = r.RoleId,
                Code = r.Code,
                RoleName = r.RoleName
            })
            .ToList();

        return ApiResponse<List<SystemRoleLookupDto>>.Ok(data);
    }

    public async Task<ApiResponse<List<BusinessRoleLookupDto>>> GetAllBusinessRolesAsync()
    {
        var rows = await _lookupRepository.GetAllBusinessRolesAsync();

        var data = rows
            .Select(r => new BusinessRoleLookupDto
            {
                BusinessRoleId = r.BusinessRoleId,
                RoleCode = r.RoleCode,
                BusinessRoleName = r.BusinessRoleName
            })
            .ToList();

        return ApiResponse<List<BusinessRoleLookupDto>>.Ok(data);
    }

    public async Task<ApiResponse<List<BusinessRoleUserLookupDto>>> GetBusinessRoleUsersAsync(
        string? roleCode,
         long? impactMasterId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _lookupRepository.GetBusinessRoleUsersAsync(roleCode, impactMasterId, cancellationToken);

        var data = rows
            .Select(r => new BusinessRoleUserLookupDto
            {
                UserId = r.UserId,
                DisplayName = r.DisplayName
            })
            .ToList();

        return ApiResponse<List<BusinessRoleUserLookupDto>>.Ok(data);
    }

    public async Task<ApiResponse<List<ProjectManagerLookupDto>>> GetProjectManagersAsync(
     string? searchTerm)
    {
        var rows = await _lookupRepository.GetProjectManagersAsync(searchTerm);

        var data = rows
            .Select(x => new ProjectManagerLookupDto
            {
                UserId = x.UserId,
                DisplayName = x.DisplayName
            })
            .ToList();

        return ApiResponse<List<ProjectManagerLookupDto>>.Ok(data);
    }
    public async Task<ApiResponse<List<ImpactCategoryLookupDto>>> GetImpactCategoryLookupAsync()
    {
        var rows = await _lookupRepository.GetImpactCategoryLookupAsync();

        var data = rows
            .Select(r => new ImpactCategoryLookupDto
            {
                CategoryId = r.CategoryId,
                CategoryName = r.CategoryName
            })
            .ToList();

        return ApiResponse<List<ImpactCategoryLookupDto>>.Ok(data);
    }
    public async Task<ApiResponse<List<PhaseLookupDto>>> GetPhaseLookupAsync()
    {
        var rows = await _lookupRepository.GetPhaseLookupAsync();

        var data = rows
            .Select(r => new PhaseLookupDto
            {
                PhaseId = r.PhaseId,
                PhaseName = r.PhaseName
            })
            .ToList();

        return ApiResponse<List<PhaseLookupDto>>.Ok(data);
    }

    public async Task<ApiResponse<List<ImpactMasterLookupDto>>> GetImpactMasterLookupAsync(
        string? searchTerm)
    {
        var rows = await _lookupRepository.GetImpactMasterLookupAsync(searchTerm);

        var data = rows
            .Select(r => new ImpactMasterLookupDto
            {
                ImpactMasterId = r.ImpactMasterId,
                Pin = r.Pin,
                Title = r.Title,
                FundingRecipientName = r.FundingRecipientName
            })
            .ToList();

        return ApiResponse<List<ImpactMasterLookupDto>>.Ok(data);
    }

    public async Task<IReadOnlyList<ReviewStageLookupDto>> GetReviewStages()
    {
        return await _lookupRepository.GetReviewStages();
    }

    public async Task<IReadOnlyList<ReviewStageLookupDto>> GetReviewStagesByUserId(long userId)
    {
        return await _lookupRepository.GetReviewStagesByUserId(userId);
    }

}
