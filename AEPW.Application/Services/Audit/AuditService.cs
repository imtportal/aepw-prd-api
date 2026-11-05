using AEPW.Application.DTOs.Audit;
using AEPW.Application.Exceptions;
using AEPW.Application.Interfaces.Repositories.Audit;
using AEPW.Application.Interfaces.Services.Audit;

namespace AEPW.Application.Services.Audit;

public sealed class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;

    public AuditService(IAuditRepository auditRepository)
    {
        _auditRepository = auditRepository;
    }

    public async Task<List<AuditTimelineItemDto>> GetAuditTimelineAsync(
        GetAuditTimelineRequestDto request,
        CancellationToken cancellationToken = default)
    {
        // 1. Set default last 30 days if FromDate/ToDate not provided
        var toDate = request.ToDate ?? DateTime.UtcNow;
        var fromDate = request.FromDate ?? toDate.AddDays(-30);

        // 2. Validate Date Range
        if (toDate < fromDate)
        {
            throw new ValidationException("ToDate cannot be earlier than FromDate.");
        }

        double totalDays = (toDate - fromDate).TotalDays;
        if (totalDays > 90)
        {
            throw new ValidationException("Audit timeline date range cannot exceed 90 days.");
        }

        // 3. Assign validated dates
        request.FromDate = fromDate;
        request.ToDate = toDate;

        // 4. Fetch from Repository
        return await _auditRepository.GetAuditTimelineAsync(request, cancellationToken);
    }
}
