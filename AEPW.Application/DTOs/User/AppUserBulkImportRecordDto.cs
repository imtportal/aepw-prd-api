using System;

namespace AEPW.Application.DTOs.User
{
    // Represents a single row for the dbo.AppUserBulkModel TVP.
    public sealed class AppUserBulkImportRecordDto
    {
        public Guid AzureAdObjectId { get; set; }
        public string Upn { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? JobTitle { get; set; }
        public string? SystemRoleCode { get; set; }
    }
}
