using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Evidence
{
    public class EvidenceDto
    {
        public long EvidenceId { get; set; }
        public long ReviewMetricId { get; set; }
        public string ItemId { get; set; }
        public string FileName { get; set; }
        public long? FileSizeBytes { get; set; }
        public string FileUriView { get; set; }
        public string FileUriDownload { get; set; }
        public string? SiteTag { get; set; }
        public string? Period { get; set; }
        public string? Language { get; set; }
        public long UploadedByUserId { get; set; }
        public DateTime UploadedUtc { get; set; }
        public int Status { get; set; }
        public bool IsLocked { get; set; }

        public string StatusDisplayValue { get; set;  }
        public string StatusTextColor { get; set;  }
        public string StatusBadgeColor { get; set;  }

        public long? EvidenceTypeId { get; set; }
        public string? EvidenceTypeCode { get; set; }
        public string? EvidenceTypeDescription { get; set; }
        

        public long CreatedBy { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }


        public decimal? Contribution { get; set; }
        public string? EvidencedValue { get; set; }

    }
}
