using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.ImpactMaster
{
    public sealed class ImpactMasterListDto
    {
        public long ImpactMasterId { get; set; }

        public string? Pin { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public long? ProgramId { get; set; }

        public string ProgramName { get; set; } = string.Empty;

        public string FundingRecipientNames { get; set; } = string.Empty;

        public long RecordTypeId { get; set; }

        public string RecordTypeName { get; set; } = string.Empty;

        public long? CategoryId { get; set; }

        public string? CategoryCode { get; set; }

        public string? CategoryName { get; set; }

        public long? PhaseId { get; set; }

        public string? PhaseCode { get; set; }

        public string? PhaseName { get; set; }

        public long? StatusId { get; set; }

        public string StatusCode { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;

        public long? L1UserId { get; set; }

        public string L1UserName { get; set; } = string.Empty;
        public string? MetricCategories { get; set; }

        public DateTime CreatedOnUTC { get; set; }

        public DateTime? UpdatedOnUTC { get; set; }
    }

    public sealed class GetImpactMastersRequestDto
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? SearchText { get; set; }

        public long? ProgramId { get; set; }
        public string? ImpactMasterId { get; set; }

        public long? FundingRecipientId { get; set; }

        public long? StatusId { get; set; }

        public long? L1UserId { get; set; }

        public long? RecordTypeId { get; set; }

        public long? PhaseId { get; set; }

        public long? CategoryId { get; set; }

        public string? MetricCategories { get; set; }
    }

    public sealed class CreateImpactMasterRequestDto
    {
        public string? Pin { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? ProgramId { get; set; }

        public List<string> FundingRecipientIds { get; set; } = [];

        public string? L1UserId { get; set; } = string.Empty;

        public string? L2ApproverUserId { get; set; }

        public string? L3ApproverUserId { get; set; }

        public string RecordTypeId { get; set; } = string.Empty;

        public string? PhaseId { get; set; }

        public string? CategoryId { get; set; }

        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }



        public decimal? TotalFunding { get; set; }

        public string? InternalReferences { get; set; }

        public string? FieldCheckReport { get; set; }

        public DateTime? LastFieldCheckDate { get; set; }

        public string? StatusId { get; set; } = string.Empty;

        public long CreatedBy { get; set; }

        public List<string> CityIds { get; set; } = [];
    }

    public sealed class CreateImpactMasterResponseDto
    {
        public long ImpactMasterId { get; set; }

        public string Pin { get; set; } = string.Empty;
    }

}
