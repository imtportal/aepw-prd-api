using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.Evidence;
using AEPW.Application.Interfaces.Services.Evidence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AEPW.API.Controllers;

// [Authorize]
[ApiController]
[Route("api/evidence")]
public class EvidenceProcessingController : ControllerBase
{
    private readonly IEvidenceProcessingService _evidenceProcessingService;

    public EvidenceProcessingController(
        IEvidenceProcessingService evidenceProcessingService)
    {
        _evidenceProcessingService = evidenceProcessingService;
    }

    /// <summary>
    /// Runs Document Intelligence extraction for an Evidence row already created by the
    /// evidence-processing Worker, and persists the extracted field values.
    ///
    /// Response contract (Layer 2 - permanent vs transient failure classification; the Worker
    /// reads this status code to decide whether to redeliver the Service Bus message):
    ///   - 200 OK                   : success. Evidence is saved and its StatusCode is set
    ///                                 (Validated/NeedsReview). Do not retry.
    ///   - 400 Bad Request          : request/file validation failed at the door (bad multipart
    ///                                 fields, unsupported file type). Evidence is NOT marked
    ///                                 Processing/Failed for this call. Do not retry as-is.
    ///   - 422 Unprocessable Entity : PERMANENT processing failure (agent refusal/non-JSON output,
    ///                                 malformed or schema-invalid agent JSON, corrupt input).
    ///                                 Evidence IS marked Failed. Do NOT redeliver this message.
    ///   - 503 Service Unavailable  : TRANSIENT processing failure (agent call timeout, transient
    ///                                 HTTP 408/429/5xx from the agent endpoint, transient SQL
    ///                                 error on the final write). Evidence is deliberately left at
    ///                                 Processing (NOT marked Failed) - SHOULD redeliver/retry this
    ///                                 message later.
    ///   - 500 Internal Server Error: unclassified/unexpected error. Evidence IS marked Failed,
    ///                                 treated the same as 422 - do not redeliver blindly.
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> ProcessEvidence(
        [FromForm] ProcessEvidenceRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _evidenceProcessingService.ProcessAsync(
            request,
            cancellationToken);

        return Ok(ApiResponse<ProcessEvidenceResponseDto>.Ok(result));
    }
}
