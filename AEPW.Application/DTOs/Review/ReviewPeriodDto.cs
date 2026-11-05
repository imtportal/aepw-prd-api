using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewPeriodDto
    {
        public int ReviewPeriodId { get; set; }
        public int Year { get; set; }
        public int? Quarter { get; set; }
        public string PeriodCode { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public DateTime SubmissionDueUtc { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedOnUTC { get; set; }


        public bool IsCurrent { get; set; }
    }
}
