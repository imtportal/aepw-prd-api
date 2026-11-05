namespace AEPW.Application.DTOs.User
{
    public sealed class BulkImportUserResultDto
    {
        public string Email { get; set; } = string.Empty;
        public long? UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }
}
