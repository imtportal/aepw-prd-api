using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Lookup
{
    public sealed class ImpactMasterLookupModel
    {
        public string ImpactMasterId { get; set; } = default!;
        public string Pin { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string FundingRecipientName { get; set; } = default!;
  
    }
}
