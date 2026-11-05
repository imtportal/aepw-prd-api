using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class SharepointHierarchyFolderCreationL1TopTopic
    {
        public string CategoryCode { get; set; }
        public string ParentMetricCode { get; set; }
        public List<L1MetricCodeEvidenceTypeCodeDto> L1MetricCodes { get; set; } = new List<L1MetricCodeEvidenceTypeCodeDto>();
        
    }
}
