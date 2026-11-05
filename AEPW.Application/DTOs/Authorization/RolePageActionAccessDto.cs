namespace AEPW.Application.DTOs.Authorization;

public sealed class RolePageActionAccessDto
{
    public long SystemRoleId { get; set; }
    public string SystemRoleName { get; set; } = string.Empty;
    public long PageStructureId { get; set; }
    public long PageActionStructureId { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string ActionKey { get; set; } = string.Empty;
    public string ActionDescription { get; set; } = string.Empty;
    public bool HasAccess { get; set; }
}
