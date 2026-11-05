using System;
using System.Collections.Generic;

namespace AEPW.Application.DTOs.ImpactMaster
{
    public sealed class UpdateImpactMasterRequestDto
    {
        public string ImpactMasterId { get; set; } = string.Empty;

        public string? Pin { get; set; } 

        public string Title { get; set; } 

        public string? ProgramId { get; set; } = string.Empty;

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

        public long UpdatedBy { get; set; } 

        public List<string> CityIds { get; set; } = [];
    }
}