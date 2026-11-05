using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Lookup
{
    public class MetricSourceLookupModel
    {
        public long MetricSourceId { get; set; }

        public string Code { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
    }
}
