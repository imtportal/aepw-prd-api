using AEPW.Domain.Models.Evidence;

namespace AEPW.Application.Interfaces.Services.Foundry;

/// <summary>
/// Wraps a single call to the Microsoft Foundry Agent Service (Responses API) that
/// performs extraction AND validation on a document in one shot. The agent is a
/// vision agent - the file itself is sent as part of the request, not just a text prompt.
/// </summary>
public interface IFoundryAgentService
{
    /// <summary>
    /// Sends <paramref name="file"/> plus context (metric/type/period) to the configured
    /// Foundry agent and returns its raw JSON output (as a string, from output_text).
    /// </summary>
    /// <param name="preprocessedFile">
    /// The caller's already-classified preprocessing result (see IFilePreprocessor). Must be
    /// ConvertedText or Passthrough - the caller is responsible for rejecting Unsupported
    /// files before ever reaching this method, so no Foundry call is made for them.
    /// </param>
    Task<FoundryResult> ProcessAsync(
        Stream file,
        string fileName,
        string mimeType,
        string metricCode,
        string evidenceType,
        int reportingYear,
        string reportingQuarter,
        PreprocessedFile preprocessedFile,
        CancellationToken cancellationToken = default);
}
