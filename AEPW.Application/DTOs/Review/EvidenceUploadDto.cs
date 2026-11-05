using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class EvidenceUploadDto
    {
        public string EvidenceTypeName { get; set; } = string.Empty;

        // "Will Upload" = true
        // "Will Not Upload" = false
        public bool WillUpload { get; set; }
    }
}
