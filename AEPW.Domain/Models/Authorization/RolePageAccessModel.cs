namespace AEPW.Domain.Models.Authorization;

public sealed class RolePageAccessModel
{
    public long SystemRoleId { get; set; }
    public string SystemRoleName { get; set; } = string.Empty;
    public long PageStructureId { get; set; }
    public string PageName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool HasAccess { get; set; }
}
