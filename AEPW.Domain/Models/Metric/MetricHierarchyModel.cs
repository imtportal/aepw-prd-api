using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Metric
{
    public sealed class MetricHierarchyModel
    {
        public string CategoryId { get; set; } = default!;

        public string CategoryCode { get; set; } = default!;

        public string CategoryName { get; set; } = default!;
        public string CategoryDescription { get; set; } = default!;
        public string? CategoryColor { get; set; }

        public string? L4ApproverUserId { get; set; }

        public string? L4ApproverName { get; set; }

        public string MetricId { get; set; } = default!;

        public string MetricCode { get; set; } = default!;

        public string MetricName { get; set; } = default!;

        public string? Description { get; set; }
        public string? MetricHeader { get; set; }

        public bool IsTopLevel { get; set; }

        public string? ParentMetricId { get; set; }

        public string? ParentMetricName { get; set; }

        public string? MetricSourceId { get; set; }

        public string? MetricSourceCode { get; set; }

        public string? MetricSourceName { get; set; }


        public bool IsRiskAdjustmentRequired { get; set; }
        public bool IsICPRequired { get; set; }

        public string? ICPApproverUserId { get; set; }

        public string? ICPApproverName { get; set; }

        public bool IsActive { get; set; }


        public string? MetricUnitId { get; set; }

        public string? UnitName { get; set; }
    }
}
