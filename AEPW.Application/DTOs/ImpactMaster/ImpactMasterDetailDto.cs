using System;
using System.Collections.Generic;
using System.Text;
using AEPW.Application.DTOs.ImpactMaster;

namespace AEPW.Application.DTOs.ImpactMaster
{
    public sealed class ImpactMasterDetailDto
    {
        public string ImpactMasterId { get; set; } = string.Empty;

        public string? Pin { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ProgramId { get; set; }

        public string? ProgramName { get; set; }

        public List<ImpactMasterFundingRecipientDto> FundingRecipients { get; set; } = [];

        public string? L1UserId { get; set; }

        public string? L1UserName { get; set; }

        public string? L2ApproverUserId { get; set; }

        public string? L2ApproverUserName { get; set; }

        public string? L3ApproverUserId { get; set; }

        public string? L3ApproverUserName { get; set; }

        public string? CorporateApproverUserId { get; set; }

        public string? CorporateApproverUserName { get; set; }

        public string RecordTypeId { get; set; } = string.Empty;

        public string RecordTypeName { get; set; } = string.Empty;

        public string? PhaseId { get; set; }

        public string? PhaseCode { get; set; }

        public string? PhaseName { get; set; }

        public string? CategoryId { get; set; }

        public string? CategoryCode { get; set; }

        public string? CategoryName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime? ActualCompletionDate { get; set; }


        public decimal? TotalFunding { get; set; }

        public string? InternalReferences { get; set; }

        public string? FieldCheckReport { get; set; }

        public DateTime? LastFieldCheckDate { get; set; }

        public string? StatusId { get; set; } = string.Empty;

        public string? StatusCode { get; set; } = string.Empty;

        public string? StatusName { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedOnUTC { get; set; }

        public string? UpdatedBy { get; set; }

        public DateTime? UpdatedOnUTC { get; set; }
        public string? DataCollectionFolderUrl { get; set; }

        public List<ImpactMasterCityDto> Cities { get; set; } = [];
    }
}
