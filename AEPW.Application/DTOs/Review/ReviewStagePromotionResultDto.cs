using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewStagePromotionResultDto
    {
        public List<ReviewPromotionNextStageDetailsDto> reviewPromotionNextStageDetails { get; set;  } = new List<ReviewPromotionNextStageDetailsDto>();
        public List<StagePromotionDetailDto> stagePromotionDetails { get; set;  } = new List<StagePromotionDetailDto>();
    }
}
