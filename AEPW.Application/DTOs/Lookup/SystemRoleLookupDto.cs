using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Lookup
{
    public sealed class SystemRoleLookupDto
    {
        public string RoleId { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
