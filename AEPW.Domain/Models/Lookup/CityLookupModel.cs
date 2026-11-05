using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Domain.Models.Lookup
{
    public sealed class CityLookupModel
    {
        public long CityId { get; set; } =  default!;
        public string CountryName { get; set; } = default!;
        public string CityName { get; set; } = default!;
    }
}