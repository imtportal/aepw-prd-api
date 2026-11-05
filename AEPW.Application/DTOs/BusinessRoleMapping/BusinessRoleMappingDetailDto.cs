using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.BusinessRoleMapping
{
    public sealed class BusinessRoleMappingDetailDto
    {
        public string BusinessRoleMappingId { get; set; } = default!;

        public string UserId { get; set; } = default!;
        public string UserName { get; set; } = default!;

        public string BusinessRoleId { get; set; } = default!;
        public string BusinessRoleName { get; set; } = default!;
        public string RoleCode { get; set; } = default!;

        public string? RegionId { get; set; }
        public string? RegionCode { get; set; }
        public string? RegionName { get; set; }

        public string? ReviewStageCode { get; set; }
        public string? ReviewStageName { get; set; }

        public string? CategoryCode { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryColor { get; set; }

        public bool IsActive { get; set; }
    }
}
