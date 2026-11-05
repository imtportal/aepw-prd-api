using AEPW.Application.DTOs.Metric;
using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Review
{
    public class LaunchableProjectDto
    {
        public long? ReviewId { get; set; }
        public long? ImpactMasterId { get; set; }
        public string Pin { get; set; }
        public string Title { get; set; }
        public long? ProgramId { get; set; }
        public string ProgramName { get; set; }
        public long? FundingRecipientId { get; set; }
        public string FundingRecipientName { get; set; }
        public string FundingRecipientOrgName { get; set; }
        public long? FundingRecipientCountryId { get; set; }
        public string FundingRecipientCountryName { get; set; }
        public string OrgName { get; set; }
        public string ContactName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public long? L1UserId { get; set; }
        public string L1UserName { get; set; }
        public long? StatusId { get; set; }
        public string StatusCode { get; set; }
        public string StatusName { get; set; }
        public long? RegionId { get; set; }
        public string? RegionName { get; set; }
        public string MetricCategories { get; set; }
        public string MetricCategoryList { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string ReportingFrequency { get; set; }
        public long? TotalFunding { get; set; }
        public long? CreatedBy { get; set; }
        public string CreatedOnUtc { get; set; }
        public long? UpdatedBy { get; set; }
        public string UpdatedOnUtc { get; set; }

        public List<MetricDto> Metrics { get; set; } = new List<MetricDto>();


        public string LastReviewStage { get; set; }
        public long? TotalMetrics { get; set; }
        public string? ReviewStatus { get; set; }
        public bool? AllowDeletion { get; set; }
        public string? DeletionPreventionReason { get; set; }

        public int PageNo { get; set; }
        public int SkippedRows { get; set; }
        public int TotalRows { get; set; }
        public int ItemsPerPage { get; set; }
    }
}
