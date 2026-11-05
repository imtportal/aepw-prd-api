using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.AzureAdUser;
namespace AEPW.Application.Interfaces.Services.AzureAd
{

    public interface IAzureAdService
    {
        Task<ApiResponse<List<AzureAdUserLookupDto>>>
            SearchUsersAsync(string term);
        Task<ApiResponse<List<AzureAdUserLookupDto>>> GetUsersByEmailsAsync(
     IReadOnlyList<string> emails);
    }
}
