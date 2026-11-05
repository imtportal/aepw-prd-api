using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.BusinessRoleMapping
{
    public sealed class SaveBusinessRoleMappingRequestDto
    {
        public string? BusinessRoleMappingId { get; set; }

        public string BusinessRoleId { get; set; } = default!;
        public string UserId { get; set; } = default!;

        public string? RegionId { get; set; }
        public string? ReviewStageCode { get; set; }

        public string? CategoryCode { get; set; }

        public bool IsActive { get; set; }
    }
    public sealed class SaveBusinessRoleMappingResponseDto
    {
        public string BusinessRoleMappingId { get; set; } = default!;
    }
}


