using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class RecallTopLevelMetricResultDto
    {
        public long? PreviousStageCode { get; set; }
        public long? NewStageCode { get; set; }
        public string Message { get; set; }

    }
}
