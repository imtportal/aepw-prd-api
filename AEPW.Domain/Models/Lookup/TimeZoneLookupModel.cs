using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Lookup
{
    public class TimeZoneLookupModel
    {
        public long TimeZoneId { get; set; }

        public string TimeZoneCode { get; set; } = string.Empty;

        public string TimeZoneName { get; set; } = string.Empty;
    }
}
