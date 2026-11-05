using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class L1MetricCodeEvidenceTypeCodeDto
    {
        public string MetricCode { get; set; }
        public string ParentMetricCode { get; set; }
        public List<string> EvidenceTypeCodes { get; set; }
    }
}
