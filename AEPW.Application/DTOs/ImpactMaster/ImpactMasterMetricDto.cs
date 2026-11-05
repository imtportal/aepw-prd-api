using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.ImpactMaster
{
    public sealed class ImpactMasterMetricDto
    {
        public string MetricId { get; set; } = string.Empty;

        public string? ImpactMasterMetricId { get; set; }

        public string CategoryId { get; set; } = string.Empty;
        public string CategoryCode { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? CategoryColor { get; set; }

        public string MetricCode { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public string? Description { get; set; }

        public bool IsTopLevel { get; set; }

        public string? ParentMetricId { get; set; }
        public string? ParentMetricName { get; set; }

        public string? MetricUnitId { get; set; }
        public string? UnitName { get; set; }

        public bool IsActive { get; set; }
        public decimal? RiskPct { get; set; }
        public decimal? BaselinePct { get; set; }

    }
    public sealed class SaveImpactMasterMetricsRequest
    {
        public string ImpactMasterId { get; set; } = string.Empty;

        public List<ImpactMasterMetricItemRequest> Metrics { get; set; } = [];
        public long UpdatedBy { get; set; }
    }
    public sealed class ImpactMasterMetricItemRequest
    {
        public string MetricId { get; set; } = string.Empty;

        public decimal? RiskPct { get; set; }

        public decimal? BaselinePct { get; set; }
        public bool IsActive { get; set; }
    }

}
