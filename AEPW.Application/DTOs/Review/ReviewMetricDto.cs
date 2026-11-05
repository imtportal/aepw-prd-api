using AEPW.Application.DTOs.Evidence;
using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class ReviewMetricDto
    {
        public Int64? ReviewMetricId { get; set; }
        public Int64? ReviewId { get; set; }
        public Int64? MetricId { get; set; }
        public Int64? ParentMetricId { get; set; }
        public string? MetricCode { get; set; }
        public string? MetricDescription { get; set; }

        public long? CategoryId { get; set; }
        public string? CategoryCode { get; set; }
        public string? Status { get; set; }
        public string? CategoryColor { get; set; }
        public string? CategoryName { get; set; }
        public string? MetricName { get; set; }
        public string? FRComment { get; set; }
        
        public int IsTopLevel { get; set; }
        public int? IsRequired { get; set; }
        public decimal? Baseline { get; set; }
        public decimal? RiskOriginal { get; set; }
        public decimal? RiskPctProposed { get; set; }
        public decimal? RiskPctEffective { get; set; }
        public string? RAStatus { get; set; }
        public int? RAApprovedByUserId { get; set; }
        public string? RAApprovedUtc { get; set; }
        public string? RAReason { get; set; }
        public string? RAReviewComment { get; set; }
        public string? ValidationStatus { get; set; }
        public int? EvidenceCount { get; set; }
        public int? EvidenceUploadCount { get; set; }
        public decimal? VarianceAbs { get; set; }
        public decimal? VariancePct { get; set; }
        public decimal? RawValue { get; set; }
        public decimal? AdjustedValue { get; set; }
        public string? LastValueSource { get; set; }
        public int? LastValueEnteredByUserId { get; set; }
        public string? LastValueEnteredUtc { get; set; }
        public int? ValorisationTypeId { get; set; }
        public bool IsValorisationRequired { get; set; }
        public bool? IsRiskAdjustmentRequired { get; set; }

        public decimal? ReportedLimitAcknowledgedBy { get; set; }
        public decimal? ReportedLimit { get; set; }
        public string? ReportedLimitMsg { get; set; }
        
        public int? RejectedInSubsequentStages { get; set; }
        public int? CurrentStage { get; set; }
        public decimal? Evidenced { get; set; }
        public decimal? EvidencedPer { get; set; }
        public string? StageDescription { get; set; }

        public bool AllowAction { get; set; }
        public bool AllowRejection { get; set; }
        public bool AllowRecall { get; set; }
        //public bool AllowRestatement { get; set; }

        public List<EvidenceDto> Evidences { get; set; } = new List<EvidenceDto>();
    }
}
