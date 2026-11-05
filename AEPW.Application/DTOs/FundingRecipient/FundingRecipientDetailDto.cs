using AEPW.Application.DTOs.FundingRecipient.Contact;

namespace AEPW.Application.DTOs.FundingRecipient
{
    public sealed class FundingRecipientDetailDto
    {
        public long FundingRecipientId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string OrgName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string ZipCode { get; set; } = string.Empty;

        public List<FundingRecipientCityDto> Cities { get; set; } = [];

        public long? ContactId { get; set; }

        public string? ContactName { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }

        public bool IsActive { get; set; }

        public long CreatedBy { get; set; }

        public DateTime CreatedOnUTC { get; set; }

        public long? UpdatedBy { get; set; }

        public DateTime? UpdatedOnUTC { get; set; }
    }
}
