namespace AEPW.Application.DTOs.Authorization
{
    public class LoginContextDto
    {
        public UserDetailsDto UserDetails { get; set; } = new();
        public List<PageAccessDto> PageAccess { get; set; } = new();
        public List<ActionPermissionDto> ActionPermissions { get; set; } = new();
    }

    public class UserDetailsDto
    {
        public long UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ActionPermissionDto
    {
        public string PageKey { get; set; } = string.Empty;
        public string ActionKey { get; set; } = string.Empty;
        public bool Value { get; set; }
    }
}
