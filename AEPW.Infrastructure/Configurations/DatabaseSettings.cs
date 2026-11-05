namespace AEPW.Infrastructure.Configurations;

/// <summary>
/// Strongly-typed binding for the "Database" section in appsettings.
/// Bind via: services.Configure&lt;DatabaseSettings&gt;(configuration.GetSection(DatabaseSettings.SectionName))
/// </summary>
public sealed class DatabaseSettings
{
    public const string SectionName = "Database";

    public string ConnectionString { get; init; } = string.Empty;

    /// <summary>Max pool size for SQL connections (Azure SQL default: 100).</summary>
    public int MaxPoolSize { get; init; } = 100;

    /// <summary>Command timeout in seconds.</summary>
    public int CommandTimeoutSeconds { get; init; } = 30;
}
