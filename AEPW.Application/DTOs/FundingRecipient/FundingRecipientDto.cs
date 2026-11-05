namespace AEPW.Application.DTOs.FundingRecipient;

public sealed class FundingRecipientDto
{
    public long FundingRecipientId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string OrgName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public string CityName { get; set; } = string.Empty;

    public string CountryName { get; set; } = string.Empty;

    public long? ContactId { get; set; }

    public string? ContactName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }
    public bool IsActive { get; set; }
}