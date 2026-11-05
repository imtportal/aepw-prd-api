using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ProjectLevelAggregationDto
    {
        public string Title { get; set; }
        public long ImpactMasterId { get; set; }
        public long RecordTypeId { get; set; }
        public string Pin { get; set; }
        public long ReviewId { get; set; }
        public string ParentCode { get; set; }
        public decimal RawValue { get; set; }
        public decimal AdjustedValue { get; set; }
        public decimal Evidenced { get; set; }
        public string Breakdown { get; set; }
        public int Total { get; set; }
    }
}
