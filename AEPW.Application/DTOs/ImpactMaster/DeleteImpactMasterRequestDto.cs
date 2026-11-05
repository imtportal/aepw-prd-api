using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.ImpactMaster
{
    public sealed class DeleteImpactMasterRequestDto
    {
        public string ImpactMasterId { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }
}
