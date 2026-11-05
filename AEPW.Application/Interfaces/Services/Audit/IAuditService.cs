namespace AEPW.Application.Interfaces.Services.Audit;

using AEPW.Application.DTOs.Audit;

public interface IAuditService
{
    Task<List<AuditTimelineItemDto>> GetAuditTimelineAsync(
        GetAuditTimelineRequestDto request,
        CancellationToken cancellationToken = default);
}
