namespace AEPW.Domain.Models.Lookup;

public sealed class FundingRecipientLookupModel
{
    public string FundingRecipientId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string OrgName { get; set; } = default!;
    //public string CountryId { get; set; } = default!;
    //public string CountryCode { get; set; } = default!;
    //public string CountryName { get; set; } = default!;
}
