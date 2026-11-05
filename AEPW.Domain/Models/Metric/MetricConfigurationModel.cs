using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Metric
{
    public sealed class MetricConfigurationModel
    {
        public long MetricId { get; set; }

        public string? Description { get; set; }

        public string? MetricHeader { get; set; }

        public long? MetricSourceId { get; set; }

        public bool IsICPRequired { get; set; }

        public bool IsRiskAdjustmentRequired { get; set; }

        public long? ICPApproverUserId { get; set; }

        public bool IsActive { get; set; }
    }
}
