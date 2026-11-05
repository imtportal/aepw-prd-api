namespace AEPW.Domain.Models.Evidence;

/// <summary>
/// Result of a Microsoft Foundry Agent Responses API call: the agent's raw JSON
/// output (as a string, from output_text).
/// </summary>
public sealed record FoundryResult(string OutputText)
{
    /// <summary>
    /// Agent name/version identifier, for use as uspEvidenceSaveRawExtraction's
    /// @ModelId (audit trail of which agent produced this extraction). Null if unavailable.
    /// </summary>
    public string? AgentIdentifier { get; init; }
}
