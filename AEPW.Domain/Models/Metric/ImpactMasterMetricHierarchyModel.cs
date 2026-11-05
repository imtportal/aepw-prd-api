using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Metric
{
    public sealed class ImpactMasterMetricHierarchyModel
    {

        public string CategoryId { get; set; } = default!;

        public string CategoryCode { get; set; } = default!;

        public string CategoryName { get; set; } = default!;

        public string? CategoryColor { get; set; }

        public string MetricId { get; set; } = default!;

        public string MetricCode { get; set; } = default!;

        public string MetricName { get; set; } = default!;

        public string? Description { get; set; }

        public bool IsTopLevel { get; set; }

        public string? ParentMetricId { get; set; }

        public string? ParentMetricName { get; set; }

        public bool IsActive { get; set; }

        public string? MetricUnitId { get; set; }

        public string? UnitName { get; set; }

        public string? ImpactMasterMetricId { get; set; }

        public decimal? RiskPct { get; set; }

        public decimal? BaselinePct { get; set; }

        //public decimal? AverageRiskPct { get; set; }

        public bool IsRiskAdjustmentRequired { get; set; }
    }
}
