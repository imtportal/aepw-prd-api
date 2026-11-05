namespace AEPW.Application.DTOs.Authorization;

public sealed class SaveRolePageAccessRequestDto
{
    public List<RolePageAccessItemDto> RolePageAccess { get; set; } = new();
    public long UpdatedBy { get; set; }
}

public sealed class RolePageAccessItemDto
{
    public long SystemRoleId { get; set; }
    public long PageStructureId { get; set; }
    public bool HasAccess { get; set; }
}
