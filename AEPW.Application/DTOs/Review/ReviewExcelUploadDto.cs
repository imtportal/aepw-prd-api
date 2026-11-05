using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewExcelUploadDto
    {
        public Int64 DocumentId { get; set; }
        public Int64 ReviewId { get; set; }
        public string ViewUri { get; set; }
        public string? DownloadUri { get; set; }
        public string FileName { get; set; }
        public Int64 UploadedByUserId { get; set; }
        public DateTime UploadedUtc { get; set; }
        public Int64 ParseStatus { get; set; }
        public string ParseLogJson { get; set; }
        public string StagedValuesJson { get; set; }
        public Int64 CreatedBy { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public Int64? UpdatedBy { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }

    }
}
