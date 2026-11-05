using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewExcelImportDto
    {
        public long ReviewId { get; set; }

        public string CategoryName { get; set; }

        public string MetricCode { get; set; }

        // User-entered value
        public string? RawValue { get; set; }

        // User-entered comment
        public string? FRComment { get; set; }
        public bool IsTopLevel { get; set; }

        // Child evidence rows
        public List<EvidenceUploadDto> Evidences { get; set; } = new();
    }
}
