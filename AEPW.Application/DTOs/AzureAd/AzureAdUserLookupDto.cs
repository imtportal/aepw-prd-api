

namespace AEPW.Application.DTOs.AzureAdUser
{
    public sealed class AzureAdUserLookupDto
    {
        public string Id { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? GivenName { get; set; }
        public string? Surname { get; set; }
        public string? UserPrincipalName { get; set; }
        public string? Mail { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public string? MobilePhone { get; set; }
        public string? City { get; set; }
    }
}
