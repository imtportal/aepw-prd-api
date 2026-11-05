using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewMetricCodeForAdminL4ApprovalRejectionDto
    {
        public long ReviewId { get; set; }
        public string MetricCode { get; set; }
        public string ParentCode { get; set; }
        public int Total { get; set; }
    }
}
