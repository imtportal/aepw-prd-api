using AEPW.Application.DTOs.FundingRecipient;
using AEPW.Application.DTOs.FundingRecipient.Contact;
using AEPW.Application.Contracts.Responses;
namespace AEPW.Application.Interfaces.Services.FundingRecipient;

public interface IFundingRecipientService
{
    Task<PagedResponse<FundingRecipientDto>> GetAllAsync(
        GetFundingRecipientsRequestDto request,
        CancellationToken cancellationToken = default);
    Task<long> CreateAsync(
    CreateFundingRecipientRequestDto request,
    CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(
    UpdateFundingRecipientRequestDto request,
    CancellationToken cancellationToken = default);
    Task<FundingRecipientDetailDto?> GetByIdAsync(
    long fundingRecipientId,
    CancellationToken cancellationToken = default);

}