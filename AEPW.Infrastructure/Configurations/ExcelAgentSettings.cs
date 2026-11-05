namespace AEPW.Infrastructure.Configurations;

/// <summary>
/// Strongly-typed binding for the "ExcelAgent" section in appsettings.
/// Bind via: services.Configure&lt;ExcelAgentSettings&gt;(configuration.GetSection(ExcelAgentSettings.SectionName))
/// This is a SEPARATE Foundry agent from FoundryAgent - a code-interpreter agent
/// (confirmed live, version 2) that processes xlsx/xlsm workbooks directly, returning the
/// same {documentResults/validation/evidenced/unclassifiedPages} JSON shape as the main agent.
/// </summary>
public sealed class ExcelAgentSettings
{
    public const string SectionName = "ExcelAgent";

    public string AgentName { get; init; } = "aepw-unsupported-file-processor";
    public string AgentVersion { get; init; } = "2";

    /// <summary>
    /// SDK client NetworkTimeout for a single extraction call. Must stay below the ~230s Azure
    /// App Service inbound request limit, with margin for JSON parsing and the SQL write
    /// afterward - 180s is the recommended ceiling. Defaults to 180 when missing/unparseable.
    /// </summary>
    public int NetworkTimeoutSeconds { get; init; } = 180;

    /// <summary>
    /// SDK-internal retry count. 0 by design: a single HTTP request window can't absorb
    /// NetworkTimeout x (1 + retries) without blowing past the platform's ~230s limit.
    /// Retries belong at the Service Bus layer, not inside one synchronous request.
    /// </summary>
    public int MaxRetries { get; init; } = 0;
}
