using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Lookup
{
    public class StatusLookupDto
    {
        public string StatusId { get; set; } = string.Empty;

        public string StatusCode { get; set; } = string.Empty;

        public string StatusName { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsDefault { get; set; }
    }
}
