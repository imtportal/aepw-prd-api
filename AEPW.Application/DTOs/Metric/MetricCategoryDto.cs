using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Metric
{
    public sealed class MetricCategoryDto
    {
        public string CategoryId { get; set; } = default!;

        public string CategoryCode { get; set; } = default!;

        public string CategoryName { get; set; } = default!;
        public string CategoryDescription { get; set; } = default!;

        public string? CategoryColor { get; set; }
        public string? L4ApproverUserId { get; set; }
        public string? L4ApproverName { get; set; }
        public List<MetricDto> Metrics { get; set; } = [];
    }
}
