using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Lookup
{
    public sealed class ImpactMasterLookupDto
    {
        public string ImpactMasterId { get; set; } = string.Empty;
        public string Pin { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string FundingRecipientName { get; set; } = string.Empty;
    }
}
