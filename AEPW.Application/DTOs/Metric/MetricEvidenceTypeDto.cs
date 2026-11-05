using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Metric
{
    public sealed class MetricEvidenceTypeDto
    {
        public string EvidenceTypeId { get; set; } = default!;

        public string Code { get; set; } = default!;

        public string Description { get; set; } = default!;

        public bool IsRequired { get; set; }
    }
}
