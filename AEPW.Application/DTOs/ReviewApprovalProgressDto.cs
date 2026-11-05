using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs
{
    public class ReviewApprovalProgressDto
    {
        public long? ReviewId { get; set; }
        public string? MetricCode { get; set; }
        public string? Stage { get; set; }
        public string? Status { get; set; }
        public string? StageDisplayName { get; set; }
        public long? AssigneeUserId { get; set; }
        public string? AssigneeUserName { get; set; }
        public string? ApprovedBy { get; set; }
        public string? ApprovedOnUTC { get; set; }
        public string? ApprovalComment { get; set; }

        public string? RejectedBy { get; set; }
        public string? RejectedOnUTC { get; set; }
        public string? RejectionComment { get; set; }

        public long? RecallDoneById { get; set; }
        public string? RecallDoneByName { get; set; }
        public string? RecallComment { get; set; }
        public string? RecallOnUtc { get; set; }
        //public string? UpdatedOnUtc { get; set; }
        //public string? Comment { get; set; }
    }
}
