using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Authorization;
using AEPW.Application.Interfaces.Repositories.Authorization;
using AEPW.Application.Interfaces.Services.Authorization;

namespace AEPW.Application.Services.Authorization;

public sealed class RoleAccessService : IRoleAccessService
{
    private readonly IRoleAccessRepository _roleAccessRepository;

    public RoleAccessService(IRoleAccessRepository roleAccessRepository)
    {
        _roleAccessRepository = roleAccessRepository;
    }

    public async Task<ApiResponse<List<RolePageAccessDto>>> GetRolePageAccessAsync(
        CancellationToken cancellationToken = default)
    {
        var rows = await _roleAccessRepository.GetRolePageAccessAsync(cancellationToken);

        var data = rows
            .Select(r => new RolePageAccessDto
            {
                SystemRoleId = r.SystemRoleId,
                SystemRoleName = r.SystemRoleName,
                PageStructureId = r.PageStructureId,
                PageName = r.PageName,
                Description = r.Description,
                HasAccess = r.HasAccess
            })
            .ToList();

        return ApiResponse<List<RolePageAccessDto>>.Ok(data);
    }

    public async Task<ApiResponse<List<RolePageActionAccessDto>>> GetRolePageActionAccessAsync(
        long pageStructureId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _roleAccessRepository.GetRolePageActionAccessAsync(
            pageStructureId,
            cancellationToken);

        var data = rows
            .Select(r => new RolePageActionAccessDto
            {
                SystemRoleId = r.SystemRoleId,
                SystemRoleName = r.SystemRoleName,
                PageStructureId = r.PageStructureId,
                PageActionStructureId = r.PageActionStructureId,
                ActionName = r.ActionName,
                ActionKey = r.ActionKey,
                ActionDescription = r.ActionDescription,
                HasAccess = r.HasAccess
            })
            .ToList();

        return ApiResponse<List<RolePageActionAccessDto>>.Ok(data);
    }

    public async Task<ApiResponse<object>> SaveRolePageAccessAsync(
        SaveRolePageAccessRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _roleAccessRepository.SaveRolePageAccessAsync(request, cancellationToken);
        return ApiResponse<object>.Ok(null, "Role page access saved successfully");
    }

    public async Task<ApiResponse<object>> SaveRolePageActionAccessAsync(
        SaveRolePageActionAccessRequestDto request,
        CancellationToken cancellationToken = default)
    {
        await _roleAccessRepository.SaveRolePageActionAccessAsync(request, cancellationToken);
        return ApiResponse<object>.Ok(null, "Role page action access saved successfully");
    }
}
