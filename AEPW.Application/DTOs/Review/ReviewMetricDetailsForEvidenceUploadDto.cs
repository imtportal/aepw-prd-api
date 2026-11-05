using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewMetricDetailsForEvidenceUploadDto
    {   
        public string ProjectPin { get; set; }
        public string Quarter { get; set; }
        public string CategoryCode { get; set; }
        public string MetricCode { get; set; }
        public string? EvidenceTypeCode { get; set; }
        public string ParentMetricCode { get; set; }
    }
}
