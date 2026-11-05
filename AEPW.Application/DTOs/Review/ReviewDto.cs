using AEPW.Application.DTOs.Evidence;
using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewDto
    {
        public Int64 ReviewId { get; set; }
        public Int64 ImpactMasterId { get; set; }
        public string ProjectName { get; set; }
        public Int64 FundingRecipientId { get; set; }
        public string FundingRecipientName { get; set; }
        public Int64 L1UserId { get; set; }
        public string PMName { get; set; }
        public string AssigneeName { get; set; }
        public Int64 ReviewPeriodId { get; set; }
        public string? ReviewPeriodCode { get; set; }
        public byte? Quarter { get; set; }
        public string Stage { get; set; }
        public string? StageName { get; set; }
        public string? StageDisplayValue { get; set; }
        public DateTime? LaunchedUtc { get; set; }
        public DateTime? SubmittedUtc { get; set; }
        public DateTime? ClosedUtc { get; set; }
        public decimal DataCompletionPct { get; set; }
        public decimal EvidenceConfirmedPct { get; set; }
        public int? IsRestated { get; set; }
        public int CreatedBy { get; set; }
        public string? RecordTypeName { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public int? UpdatedBy { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }
        public string? UpdatedOnStr { get; set; }
        public string? DataCollectionFolderUrl { get; set; }

        public bool AllowRestatement { get; set; }
        public bool AtLeastOneMetricAssigned { get; set; }

        public List<ReviewMetricDto> ReviewMetrics { get; set; } = new List<ReviewMetricDto>();
        public List<ReviewCommentDto> Comments { get; set; } = new List<ReviewCommentDto>();
        public List<EvidenceDto> Evidences { get; set; } = new List<EvidenceDto>();
    }
}
