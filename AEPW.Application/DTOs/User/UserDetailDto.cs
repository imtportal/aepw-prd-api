namespace AEPW.Application.DTOs.User
{
    public class UserDetailDto
    {
        public string UserId { get; set; } = string.Empty;
        public string AzureAdObjectId { get; set; } = string.Empty;
        public string Upn { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;

        public long TimeZoneId { get; set; }
        //public string TimeZoneName { get; set; } = string.Empty;

        public string SystemRoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public DateTime? LastLoginUtc { get; set; }
    }
}
