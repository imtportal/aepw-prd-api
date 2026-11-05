using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.BusinessRoleMapping
{
    public sealed class SaveBusinessRoleMappingModel
    {
        public string? BusinessRoleMappingId { get; set; }

        public string BusinessRoleId { get; set; } = default!;
        public string UserId { get; set; } = default!;

        public string? RegionId { get; set; }
        public string? ReviewStageCode { get; set; }

        public string? CategoryCode { get; set; }

        public bool IsActive { get; set; }

        public string CreatedBy { get; set; } = default!;
    }
    public sealed class DeleteBusinessRoleMappingModel
    {
        public long BusinessRoleMappingId { get; set; }

        public long UpdatedBy { get; set; }
    }
}
