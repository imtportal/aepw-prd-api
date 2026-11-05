using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Metric
{
    public sealed class MetricEvidenceTypeModel
    {
        public string MetricId { get; set; } = default!;

        public string EvidenceTypeId { get; set; } = default!;

        public string Code { get; set; } = default!;

        public string Description { get; set; } = default!;

        public bool IsRequired { get; set; }
    }
}
