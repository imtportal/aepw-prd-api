using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class GetReviewFilters
    {
        public string? Project { get; set; }
        public string? PM { get; set; }
        public string? Stage { get; set; }
        public string? ReviewPeriodId { get; set; }

        public string? SortBy { get; set; }
        public int? SortDesc { get; set; }
    }
}
