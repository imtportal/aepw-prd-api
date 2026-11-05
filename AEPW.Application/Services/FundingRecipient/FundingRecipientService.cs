using AEPW.Application.DTOs.FundingRecipient;
using AEPW.Application.DTOs.FundingRecipient.Contact;
using AEPW.Application.Interfaces.Repositories.FundingRecipient;
using AEPW.Application.Interfaces.Services.FundingRecipient;
using AEPW.Application.Contracts.Responses;
namespace AEPW.Application.Services.FundingRecipient;

public sealed class FundingRecipientService : IFundingRecipientService
{
    private readonly IFundingRecipientRepository _repository;

    public FundingRecipientService(
        IFundingRecipientRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResponse<FundingRecipientDto>> GetAllAsync(
        GetFundingRecipientsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(request, cancellationToken);
    }
    public Task<long> CreateAsync(
    CreateFundingRecipientRequestDto request,
    CancellationToken cancellationToken = default)
    {
        return _repository.CreateAsync(
            request,
            cancellationToken);
    }
    public Task<bool> UpdateAsync(
    UpdateFundingRecipientRequestDto request,
    CancellationToken cancellationToken = default)
    {
        return _repository.UpdateAsync(
            request,
            cancellationToken);
    }
    public Task<FundingRecipientDetailDto?> GetByIdAsync(
    long fundingRecipientId,
    CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(
            fundingRecipientId,
            cancellationToken);
    }
    //Contact


}