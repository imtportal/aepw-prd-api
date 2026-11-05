using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class StagePromotionDetailDto
    {
        public long ReviewId { get; set; }
        public long ApprovedCount { get; set; }
        public long TotalCount { get; set; }
        public bool PromotionEligible { get; set; }
    }
}
