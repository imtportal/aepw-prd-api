using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class UpdateReviewMetricsValuesDto
    {   
        public long MetricId { get; set; }
        public decimal? Baseline { get; set; }
        public decimal? RawValue { get; set; }
        public decimal? RiskPercentage { get; set; }
        public string? RestatementComment { get; set; }

        public bool UpdatingRiskPercentage { get; set; }
    }
}
