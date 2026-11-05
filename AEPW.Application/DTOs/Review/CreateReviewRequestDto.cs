using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class CreateReviewRequestDto
    {
        public long? ReviewId { get; set; }
        public int ProjectId { get; set; }
        public int ReviewPeriodId { get; set; }
        public required string Stage { get; set; }
        public DateTime? LaunchedUtc { get; set; }
        public DateTime? SubmittedUtc { get; set; }
        public DateTime? ClosedUtc { get; set; }
        public decimal? DataCompletionPct { get; set; }
        public decimal? EvidenceConfirmedPct { get; set; }
        public bool? IsRestated { get; set; }
        public int UserId { get; set; }
        public List<CreateReviewMetricJsonDto> ReviewMetrics { get; set; } = new List<CreateReviewMetricJsonDto>();
    }
}
