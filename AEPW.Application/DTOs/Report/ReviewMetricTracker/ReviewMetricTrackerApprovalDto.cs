namespace AEPW.Application.DTOs.Report.ReviewMetricTracker
{
    public class ReviewMetricTrackerApprovalDto
    {
        public long? ReviewApprovalId { get; set; }

        public int? ReviewStageCode { get; set; }

        public string? ReviewStageName { get; set; }

        public int? StepNo { get; set; }

        public int? AttemptNo { get; set; }

        public string? Status { get; set; }

        public long? AssignedUserId { get; set; }

        public DateTime? AssignedUtc { get; set; }

        public long? DecisionByUserId { get; set; }

        public DateTime? DecisionUtc { get; set; }

        public string? Comment { get; set; }

        public bool IsCurrent { get; set; }
    }
}
