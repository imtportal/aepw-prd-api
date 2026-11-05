using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Lookup
{
    public sealed class FundingRecipientLookupDto
    {
        public string FundingRecipientId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string OrgName { get; set; } = string.Empty;

        //public string CountryId { get; set; } = string.Empty;

        //public string CountryCode { get; set; } = string.Empty;

        //public string CountryName { get; set; } = string.Empty;
    }
}
