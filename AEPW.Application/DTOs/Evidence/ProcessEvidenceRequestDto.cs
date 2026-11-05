using Microsoft.AspNetCore.Http;

namespace AEPW.Application.DTOs.Evidence;

/// <summary>
/// multipart/form-data request bound from POST /api/evidence/process.
/// </summary>
public sealed class ProcessEvidenceRequestDto
{
    public IFormFile File { get; set; } = default!;
    public long EvidenceId { get; set; }
    public string EvidenceType { get; set; } = string.Empty;

    /// <summary>Metric code the EvidenceType lookup is scoped to - required for resolution.</summary>
    public string TlmMetricCode { get; set; } = string.Empty;

    public int ReportingYear { get; set; }
    public string ReportingQuarter { get; set; } = string.Empty;

    /// <summary>Diagnostic only - not used for lookup or validation.</summary>
    public string L1MetricCode { get; set; } = string.Empty;
}
