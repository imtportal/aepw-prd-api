using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewMetricApproveRejectDto
    {
        public long ReviewId { get; set; }
		public string MetricCode { get; set; }
        public long? MetricId { get; set; }
		public string Decision { get; set; }
		public string? Comment { get; set; }

        public long? StatusId { get; set; }
        public string? Action { get; set; }

        public string? StageCode { get; set; }

        public string? Role { get; set; }
    }
}
