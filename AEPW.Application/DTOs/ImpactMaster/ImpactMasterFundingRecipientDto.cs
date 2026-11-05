using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.ImpactMaster
{
    public sealed class ImpactMasterFundingRecipientDto
    {
        public string FundingRecipientId { get; set; } = string.Empty;

        public string FundingRecipientName { get; set; } = string.Empty;

        public string FundingRecipientOrgName { get; set; } = string.Empty;
    }
}
