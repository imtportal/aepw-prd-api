using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class TopLevelMetricAggregationDto
    {
        public string ParentCode { get; set; }
        public string ParentName { get; set; }
        public decimal  RawValue { get; set; }
        public decimal AdjustedValue { get; set; }
        public decimal Evidenced { get; set; }
        public string Unit { get; set; }
    }
}
