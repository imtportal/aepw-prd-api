using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Lookup
{
    public sealed class BusinessRoleLookupModel
    {
        public string BusinessRoleId { get; set; } = default!;
        public string RoleCode { get; set; } = default!;
        public string BusinessRoleName { get; set; } = default!;
    }
}
