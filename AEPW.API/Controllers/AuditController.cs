using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Audit;
using AEPW.Application.Interfaces.Services.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AEPW.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet("timeline")]
    public async Task<IActionResult> GetAuditTimeline(
        [FromQuery] GetAuditTimelineRequestDto request,
        CancellationToken cancellationToken)
    {
        // Read X-User-Id header injected by the frontend auth interceptor.
        // This is used by each audit SP to resolve the user's timezone via
        // AppUser → TimeZone JOIN (instead of accepting a timezone string directly).
        if (Request.Headers.TryGetValue("X-User-Id", out var userIdHeader)
            && int.TryParse(userIdHeader, out var userId))
        {
            request.UserId = userId;
        }

        var result = await _auditService.GetAuditTimelineAsync(request, cancellationToken);
        return Ok(ApiResponse<object>.Ok(result));
    }
}
