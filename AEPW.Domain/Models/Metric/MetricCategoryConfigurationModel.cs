using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Metric
{
    public sealed class MetricCategoryConfigurationModel
    {
        public long CategoryId { get; set; }

        public string? CategoryDescription { get; set; }

        public long? L4ApproverUserId { get; set; }
    }
}
