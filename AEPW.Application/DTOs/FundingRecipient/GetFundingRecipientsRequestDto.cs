using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.FundingRecipient
{
    public sealed class GetFundingRecipientsRequestDto
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? SearchText { get; set; }

        public long? FundingRecipientId { get; set; }

        public long? CityId { get; set; }

        public bool? IsActive { get; set; }
    }
}
