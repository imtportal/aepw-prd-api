using AEPW.Application.DTOs.Authorization;
using AEPW.Domain.Models.Authorization;

namespace AEPW.Application.Interfaces.Repositories.Authorization;

public interface IRoleAccessRepository
{
    Task<IReadOnlyList<RolePageAccessModel>> GetRolePageAccessAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RolePageActionAccessModel>> GetRolePageActionAccessAsync(
        long pageStructureId,
        CancellationToken cancellationToken = default);

    Task SaveRolePageAccessAsync(
        SaveRolePageAccessRequestDto request,
        CancellationToken cancellationToken = default);

    Task SaveRolePageActionAccessAsync(
        SaveRolePageActionAccessRequestDto request,
        CancellationToken cancellationToken = default);
}
