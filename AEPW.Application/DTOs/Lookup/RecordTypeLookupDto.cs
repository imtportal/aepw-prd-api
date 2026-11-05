using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.Lookup
{
    public sealed class RecordTypeLookupDto
    {
        public long RecordTypeId { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
