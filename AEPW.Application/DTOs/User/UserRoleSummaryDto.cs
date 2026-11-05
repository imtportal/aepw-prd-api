namespace AEPW.Application.DTOs.User
{
    public sealed class UserRoleSummaryDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}
