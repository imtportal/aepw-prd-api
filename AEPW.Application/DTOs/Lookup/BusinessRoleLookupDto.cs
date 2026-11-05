using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Lookup
{
    public class BusinessRoleLookupDto
    {
        public string BusinessRoleId { get; set; } = default!;
        public string RoleCode { get; set; } = default!;
        public string BusinessRoleName { get; set; } = default!;
    }
}
