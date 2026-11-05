using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.BusinessRoleMapping;
using AEPW.Application.Interfaces.Repositories.BusinessRoleMapping;
using AEPW.Application.Interfaces.Services.BusinessRoleMapping;
using AEPW.Domain.Models.BusinessRoleMapping;
using AEPW.Application.DTOs.Lookup;
namespace AEPW.Application.Services.BusinessRoleMapping;

public sealed class BusinessRoleMappingService : IBusinessRoleMappingService
{
    private readonly IBusinessRoleMappingRepository _businessRoleMappingRepository;

    public BusinessRoleMappingService(
        IBusinessRoleMappingRepository businessRoleMappingRepository)
    {
        _businessRoleMappingRepository = businessRoleMappingRepository;
    }
    public async Task<ApiResponse<List<BusinessRoleMappingLookupDto>>> GetAllBusinessRoleMappingsAsync(long? userId)
    {
        var rows = await _businessRoleMappingRepository.GetAllBusinessRoleMappingsAsync(userId);

        var data = rows
            .Select(r => new BusinessRoleMappingLookupDto
            {
                BusinessRoleMappingId = r.BusinessRoleMappingId,

                UserId = r.UserId,
                UserName = r.UserName,

                BusinessRoleId = r.BusinessRoleId,
                BusinessRoleName = r.BusinessRoleName,
                RoleCode = r.RoleCode,

                RegionId = r.RegionId,
                RegionCode = r.RegionCode,
                RegionName = r.RegionName,

                ReviewStageCode = r.ReviewStageCode,
                ReviewStageName = r.ReviewStageName,

                CategoryCode = r.CategoryCode,
                CategoryName = r.CategoryName,
                CategoryColor = r.CategoryColor,

                IsActive = r.IsActive
            })
            .ToList();

        return ApiResponse<List<BusinessRoleMappingLookupDto>>.Ok(data);
    }
    public async Task<ApiResponse<SaveBusinessRoleMappingResponseDto>>
        SaveBusinessRoleMappingAsync(
            SaveBusinessRoleMappingRequestDto request)
    {
        var model = new SaveBusinessRoleMappingModel
        {
            BusinessRoleMappingId = request.BusinessRoleMappingId,
            BusinessRoleId = request.BusinessRoleId,
            UserId = request.UserId,
            RegionId = request.RegionId,
            ReviewStageCode = request.ReviewStageCode,
            CategoryCode = request.CategoryCode,
            IsActive = request.IsActive,
            CreatedBy = request.UserId
        };

        var businessRoleMappingId =
            await _businessRoleMappingRepository
                .SaveBusinessRoleMappingAsync(model);

        return ApiResponse<SaveBusinessRoleMappingResponseDto>.Ok(
            new SaveBusinessRoleMappingResponseDto
            {
                BusinessRoleMappingId = businessRoleMappingId
            });
    }

    public async Task<ApiResponse<BusinessRoleMappingDetailDto>>
        GetBusinessRoleMappingByIdAsync(
            string businessRoleMappingId)
    {
        var row =
            await _businessRoleMappingRepository
                .GetBusinessRoleMappingByIdAsync(businessRoleMappingId);

        if (row is null)
        {
            return ApiResponse<BusinessRoleMappingDetailDto>
                .Fail("Business role mapping not found.");
        }

        var data = new BusinessRoleMappingDetailDto
        {
            BusinessRoleMappingId = row.BusinessRoleMappingId,
            UserId = row.UserId,
            UserName = row.UserName,
            BusinessRoleId = row.BusinessRoleId,
            BusinessRoleName = row.BusinessRoleName,
            RoleCode = row.RoleCode,
            RegionId = row.RegionId,
            RegionCode = row.RegionCode,
            RegionName = row.RegionName,
            ReviewStageCode = row.ReviewStageCode,
            ReviewStageName = row.ReviewStageName,
            CategoryCode = row.CategoryCode,
            CategoryName = row.CategoryName,
            CategoryColor = row.CategoryColor,
            IsActive = row.IsActive
        };

        return ApiResponse<BusinessRoleMappingDetailDto>.Ok(data);
    }


    public async Task<ApiResponse<DeleteBusinessRoleMappingResponseDto>>
        DeleteBusinessRoleMappingAsync(
            long businessRoleMappingId,
            long updatedBy)
    {
        var model = new DeleteBusinessRoleMappingModel
        {
            BusinessRoleMappingId = businessRoleMappingId,
            UpdatedBy = updatedBy
        };

        var isDeleted =
            await _businessRoleMappingRepository
                .DeleteBusinessRoleMappingAsync(model);

        if (!isDeleted)
        {
            return ApiResponse<DeleteBusinessRoleMappingResponseDto>
                .Fail("Business role mapping not found or already inactive.");
        }

        return ApiResponse<DeleteBusinessRoleMappingResponseDto>.Ok(
            new DeleteBusinessRoleMappingResponseDto
            {
                IsDeleted = true
            });
    }

}