using AEPW.Application.DTOs.Evidence;

namespace AEPW.Application.Interfaces.Services.Evidence;

/// <summary>
/// Runs Document Intelligence extraction for a previously-created Evidence row and
/// persists the extracted field values, raw model response, and review flag.
/// </summary>
public interface IEvidenceProcessingService
{
    Task<ProcessEvidenceResponseDto> ProcessAsync(
        ProcessEvidenceRequestDto request,
        CancellationToken cancellationToken = default);
}
