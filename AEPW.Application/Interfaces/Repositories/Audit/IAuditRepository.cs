namespace AEPW.Application.Interfaces.Repositories.Audit;

using AEPW.Application.DTOs.Audit;

public interface IAuditRepository
{
    Task<List<AuditTimelineItemDto>> GetAuditTimelineAsync(
        GetAuditTimelineRequestDto request,
        CancellationToken cancellationToken = default);
}
