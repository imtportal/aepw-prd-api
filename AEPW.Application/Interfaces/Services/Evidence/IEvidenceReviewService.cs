using AEPW.Application.DTOs.Evidence;

namespace AEPW.Application.Interfaces.Services.Evidence;

/// <summary>
/// Builds the Angular evidence-review page's response payload for one Evidence row.
/// </summary>
public interface IEvidenceReviewService
{
    Task<EvidenceReviewResponseDto> GetReviewDetailsAsync(
        long evidenceId,
        long userId,
        CancellationToken cancellationToken = default);

    Task<bool> SaveExtractedFieldsAsync(
        long routeEvidenceId,
        SaveEvidenceReviewExtractedFieldsRequestDto request,
        CancellationToken cancellationToken = default);
}
