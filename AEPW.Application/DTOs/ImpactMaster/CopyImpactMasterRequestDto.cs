using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.ImpactMaster
{
    public sealed class CopyImpactMasterRequestDto
    {
        public string ImpactMasterId { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
    }
}
