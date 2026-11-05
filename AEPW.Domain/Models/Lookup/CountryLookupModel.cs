namespace AEPW.Domain.Models.Lookup;

public sealed class CountryLookupModel
{
    public string CountryId { get; set; } = default!;
    public string CountryCode { get; set; } = default!;
    public string CountryName { get; set; } = default!;
    public string RegionId { get; set; } = default!;
    public string RegionName { get; set; } = default!;
}
