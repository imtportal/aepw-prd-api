using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.FundingRecipient.Contact
{
    public sealed class FundingRecipientContactRequestDto
    {
        public long? ContactId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public bool IsPrimary { get; set; }
    }
}
