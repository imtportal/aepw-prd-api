using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.BusinessRoleMapping;
using AEPW.Application.DTOs.Lookup;
namespace AEPW.Application.Interfaces.Services.BusinessRoleMapping;

public interface IBusinessRoleMappingService
{

    Task<ApiResponse<List<BusinessRoleMappingLookupDto>>> GetAllBusinessRoleMappingsAsync(long? userId);
    Task<ApiResponse<SaveBusinessRoleMappingResponseDto>>
        SaveBusinessRoleMappingAsync(
            SaveBusinessRoleMappingRequestDto request);

    Task<ApiResponse<BusinessRoleMappingDetailDto>>
        GetBusinessRoleMappingByIdAsync(
            string businessRoleMappingId);


    Task<ApiResponse<DeleteBusinessRoleMappingResponseDto>>
        DeleteBusinessRoleMappingAsync(
            long businessRoleMappingId,
            long updatedBy);

}