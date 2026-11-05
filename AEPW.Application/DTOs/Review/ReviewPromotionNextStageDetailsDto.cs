using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewPromotionNextStageDetailsDto
    {
        public long ReviewId { get; set; }
        public int? PrevStageCode { get; set; }
        public string? PrevStageName { get; set; }
        public int? CurrentStageCode { get; set; }
        public int? NextStageCode { get; set; }
        public string? NextStageName { get; set; }
        public bool PromotionEligible { get; set; }
    }
}
