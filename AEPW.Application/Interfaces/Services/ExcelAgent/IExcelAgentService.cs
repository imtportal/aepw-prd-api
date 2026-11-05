using AEPW.Domain.Models.Evidence;

namespace AEPW.Application.Interfaces.Services.ExcelAgent;

/// <summary>
/// Wraps a single, stateless Responses API call to the "aepw-unsupported-file-processor"
/// Foundry agent - a code-interpreter agent that processes xlsx/xlsm workbooks directly
/// (via a Foundry Search Knowledge Base of evidence-extraction rules) and returns the SAME
/// {documentResults/pages/data, validation, evidenced, unclassifiedPages} JSON shape as the
/// main FoundryAgentService, so the existing parse/save pipeline is reused unchanged.
/// NOT YET WIRED into EvidenceProcessingService - the ClosedXML preprocessing path remains
/// the active xlsx handler until this is cut over explicitly.
/// </summary>
public interface IExcelAgentService
{
    /// <summary>
    /// Sends the raw xlsx/xlsm file (unconverted - the agent's own code interpreter reads it)
    /// plus the KB lookup key (Top Level Metric / L1 Metric / Evidence Type) and reporting
    /// period to the Excel agent, and returns its raw JSON output (from output_text).
    /// </summary>
    Task<FoundryResult> ProcessAsync(
        Stream file,
        string fileName,
        string topLevelMetric,
        string l1Metric,
        string evidenceType,
        int reportingYear,
        string reportingQuarter,
        CancellationToken cancellationToken = default);
}
