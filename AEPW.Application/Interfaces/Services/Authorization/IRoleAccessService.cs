using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Authorization;

namespace AEPW.Application.Interfaces.Services.Authorization;

public interface IRoleAccessService
{
    Task<ApiResponse<List<RolePageAccessDto>>> GetRolePageAccessAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<RolePageActionAccessDto>>> GetRolePageActionAccessAsync(
        long pageStructureId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object>> SaveRolePageAccessAsync(
        SaveRolePageAccessRequestDto request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object>> SaveRolePageActionAccessAsync(
        SaveRolePageActionAccessRequestDto request,
        CancellationToken cancellationToken = default);
}
