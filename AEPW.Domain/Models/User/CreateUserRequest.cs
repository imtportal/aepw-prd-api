using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.User
{
    public sealed class CreateUserRequest
    {
        public Guid AzureAdObjectId { get; set; }

        public string Upn { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Phone { get; set; }

        public string? JobTitle { get; set; }

        public string SystemRoleId { get; set; }

        public long TimeZoneId { get; set; }
        public string CreatedBy { get; set; }
        public bool IsActive { get; set; }

        public DateTime? LastLoginUtc { get; set; }

    }
}