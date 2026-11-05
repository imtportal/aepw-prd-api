using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class CreateReviewMetricJsonDto
    {
        public int? ReviewMetricId { get; set; }
        public int? ReviewId { get; set; }
        public int? MetricId { get; set; }
        public int? CategoryId { get; set; }
        public int? IsRequired { get; set; }
        public float? RiskPctProposed { get; set; }
        public float? RiskPctEffective { get; set; }
        public string? RAStatus { get; set; }
        public int? RAApprovedByUserId { get; set; }
        public string? RAApprovedUtc { get; set; }
        //public string? RAReason { get; set; }
        public string? RaReason { get; set; }
        public string? ValidationStatus { get; set; }
        public int? EvidenceCount { get; set; }
        public decimal? VarianceAbs { get; set; }
        public decimal? VariancePct { get; set; }
        public decimal? RawValue { get; set; }
        public decimal? AdjustedValue { get; set; }
        public string? LastValueSource { get; set; }
        public int? LastValueEnteredByUserId { get; set; }
        public string? LastValueEnteredUtc { get; set; }
    }
}
