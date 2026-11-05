

namespace AEPW.Application.DTOs.Lookup
{
    public sealed class CountryLookupDto
    {
        public string CountryName { get; set; } = string.Empty;
        public List<CityLookupDto> Cities { get; set; } = [];
    }
}
