using AEPW.Domain.Models.Evidence;

namespace AEPW.Application.Interfaces.Repositories.Evidence;

/// <summary>
/// Data access for the Evidence Review read endpoint. Backed by a single stored
/// procedure (<c>dbo.uspEvidenceGetReviewDetails</c>) returning multiple result sets.
/// </summary>
public interface IEvidenceReviewRepository
{
    /// <summary>
    /// Loads the full review payload for one Evidence row, or null when the EvidenceId
    /// does not exist.
    /// </summary>
    Task<EvidenceReviewData?> GetEvidenceReviewAsync(
        long evidenceId,
        long userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves edited scalar extracted fields for one Evidence record in a single
    /// stored-procedure operation.
    /// </summary>
    Task SaveExtractedFieldsAsync(
        long evidenceId,
        IReadOnlyList<EvidenceReviewExtractedField> fields,
        long updatedBy,
        string? evidencedValue,
        CancellationToken cancellationToken = default);
}
