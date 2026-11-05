using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Lookup
{
    public sealed class RecordTypeLookupModel
    {
        public long RecordTypeId { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}
