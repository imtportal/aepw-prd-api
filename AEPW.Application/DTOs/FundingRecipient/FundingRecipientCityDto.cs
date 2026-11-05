using System;
using System.Collections.Generic;
using System.Text;

namespace AEPW.Application.DTOs.FundingRecipient
{
    public sealed class FundingRecipientCityDto
    {
        public long FundingRecipientCityId { get; set; }

        public long CityId { get; set; }

        public string CityName { get; set; } = string.Empty;

        public string CountryName { get; set; } = string.Empty;
    }
}
