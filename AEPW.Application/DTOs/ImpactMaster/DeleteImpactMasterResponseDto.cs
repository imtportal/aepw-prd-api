using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.ImpactMaster
{
    public sealed class DeleteImpactMasterResponseDto
    {
        public string ImpactMasterId { get; set; }
        public bool IsDeleted { get; set; }
        public bool HasPendingReviews { get; set; }
        public string? PendingReviewIds { get; set; }
    }
}
