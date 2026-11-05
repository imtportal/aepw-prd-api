using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class SaveReviewStageRequest
    {
        public long ReviewStageId { get; set; }
        public bool IsActive { get; set; }
    }
}
