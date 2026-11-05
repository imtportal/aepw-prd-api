using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewMetricEvidencesJsonDto
    {
        public long MetricId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string FileUriView { get; set; } = string.Empty;
        required public string ItemId { get; set; } 
        public string FileUriDownload { get; set; } = string.Empty;
        public string? SiteTag { get; set; } = string.Empty;
        public string? Language { get; set; } = string.Empty;
        public string? Period { get; set; } = string.Empty;

        public long? EvidenceTypeId { get; set; }
    }
}
