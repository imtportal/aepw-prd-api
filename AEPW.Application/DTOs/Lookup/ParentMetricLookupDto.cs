using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Lookup
{
    public class ParentMetricLookupDto
    {
        public string MetricId { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string CategoryId { get; set; } = string.Empty;

        public bool IsCorporate { get; set; }

        public decimal? DefaultRiskPct { get; set; }

        public int EvidenceRequiredMin { get; set; }
    }
}
