using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class RecallTopLevelMetricInput
    {
        public string TopLevelMetricCode { get; set; }
        public long ReviewId  { get; set; }
	    public string Recallcomment { get; set; }
        public long UserId { get; set; }

    }
}
