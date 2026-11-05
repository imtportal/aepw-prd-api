using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Metric
{
    public sealed class MetricConfigurationDto
    {
        public string MetricId { get; set; }

        public string? Description { get; set; }

        public string? MetricHeader { get; set; }

        public string? MetricSourceId { get; set; }

        public bool IsICPRequired { get; set; }

        public bool IsRiskAdjustmentRequired { get; set; }

        public string? ICPApproverUserId { get; set; }

        public bool IsActive { get; set; }
    }
}
