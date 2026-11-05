using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Metric
{
    public sealed class MetricHierarchyResultModel
    {
        public List<MetricHierarchyModel> Metrics { get; set; } = [];

        public List<MetricEvidenceTypeModel> EvidenceTypes { get; set; } = [];
    }
}
