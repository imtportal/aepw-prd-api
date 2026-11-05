namespace AEPW.Application.DTOs.User
{
    public sealed class UpdateUserRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string SystemRoleId { get; set; } = string.Empty;
        public long TimeZoneId { get; set; }
        public bool IsActive { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
