using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class UpdateReviewMetricFRCommentDto
    {
        public long ReviewId { get; set; }
        public long? MetricId { get; set; }
        public string? MetricCode { get; set; }
        public string Comment { get; set; }
        public long UserId { get; set; }
    }
}
