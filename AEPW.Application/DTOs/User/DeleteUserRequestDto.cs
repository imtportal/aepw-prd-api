namespace AEPW.Application.DTOs.User
{
    public sealed class DeleteUserRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
