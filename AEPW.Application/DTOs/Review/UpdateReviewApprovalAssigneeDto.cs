using System;

namespace AEPW.Application.DTOs.Review
{
    public class UpdateReviewApprovalAssigneeDto
    {
        public long ReviewId { get; set; }
        public int ReviewStageCode { get; set; }
        public string MetricCode { get; set; }
        public long CurrentAssigneeUserId { get; set; }
        public long NewAssigneeUserId { get; set; }
        public long ReassignedByUserId { get; set; }
        public long UpdatedBy { get; set; }
    }
}