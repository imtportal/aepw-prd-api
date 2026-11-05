namespace AEPW.Application.DTOs.Authorization;

public sealed class SaveRolePageActionAccessRequestDto
{
    public List<RolePageActionAccessItemDto> RolePageActionAccess { get; set; } = new();
    public long UpdatedBy { get; set; }
}

public sealed class RolePageActionAccessItemDto
{
    public long SystemRoleId { get; set; }
    public long PageActionStructureId { get; set; }
    public bool CanPerformAction { get; set; }
}
