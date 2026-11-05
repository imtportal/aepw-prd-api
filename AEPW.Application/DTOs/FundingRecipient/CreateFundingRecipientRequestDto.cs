using AEPW.Application.DTOs.FundingRecipient;
namespace AEPW.Application.DTOs.FundingRecipient.Contact;

public sealed class CreateFundingRecipientRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string OrgName { get; set; } = string.Empty;

    public string? ContactName { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string Address { get; set; } = string.Empty;

    public string ZipCode { get; set; } = string.Empty;

    public List<long> CityIds { get; set; } = [];

    public bool IsActive { get; set; }

    public long CreatedBy { get; set; }

}
