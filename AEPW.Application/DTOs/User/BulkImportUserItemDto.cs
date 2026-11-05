namespace AEPW.Application.DTOs.User
{
    public sealed class BulkImportUserItemDto
    {
        public string Email { get; set; } = string.Empty;
        public string? SystemRoleCode { get; set; }
    }
}
