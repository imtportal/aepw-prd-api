using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Metric
{
    public sealed class MetricCategoryConfigurationDto
    {
        public string CategoryId { get; set; }

        public string? CategoryDescription { get; set; }

        public string? L4ApproverUserId { get; set; }
    }
}
