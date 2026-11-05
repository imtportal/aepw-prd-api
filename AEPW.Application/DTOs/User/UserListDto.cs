namespace AEPW.Application.DTOs.User
{
    public class UserListDto
    {
        public string UserId { get; set; }
        public string DisplayName { get; set; }
        public string Upn { get; set; }
        public string Email { get; set; }
        public string AzureAdObjectId { get; set; }
        public string SystemRoleId { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }

        public DateTime? LastLoginUtc { get; set; }
    }
}
