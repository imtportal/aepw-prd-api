namespace AEPW.Infrastructure.Configurations;

/// <summary>
/// Strongly-typed binding for the "FoundryAgent" section in appsettings.
/// Bind via: services.Configure&lt;FoundryAgentSettings&gt;(configuration.GetSection(FoundryAgentSettings.SectionName))
/// Authenticates via DefaultAzureCredential (managed identity) - no key is configured here.
/// </summary>
public sealed class FoundryAgentSettings
{
    public const string SectionName = "FoundryAgent";

    /// <summary>The Foundry project endpoint, e.g. https://<resource>.services.ai.azure.com/api/projects/<project>.</summary>
    public string Endpoint { get; init; } = string.Empty;

    public string AgentName { get; init; } = string.Empty;
    public string AgentVersion { get; init; } = string.Empty;

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
