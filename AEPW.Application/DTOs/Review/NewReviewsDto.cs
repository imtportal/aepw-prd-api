using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class NewReviewsDto
    {   
        public long ReviewId { get; set; }
        public long ImpactMasterId { get; set; }
        public string ProjectTitle { get; set; }
        public string? ProjectPin { get; set; }
        public string Quarter { get; set; }
        public string RecordType { get; set; }
    }
}
