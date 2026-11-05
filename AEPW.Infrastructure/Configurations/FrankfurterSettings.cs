namespace AEPW.Infrastructure.Configurations;

/// <summary>
/// Strongly-typed binding for the "Frankfurter" section in appsettings.
/// Bind via: services.Configure&lt;FrankfurterSettings&gt;(configuration.GetSection(FrankfurterSettings.SectionName))
/// No API key - Frankfurter is a free, unauthenticated ECB-rate API.
/// </summary>
public sealed class FrankfurterSettings
{
    public const string SectionName = "Frankfurter";

    public string BaseUrl { get; init; } = "https://api.frankfurter.dev";
    public int TimeoutSeconds { get; init; } = 10;
}
