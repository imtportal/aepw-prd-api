using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AEPW.API.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        var connectionString = configuration["Database:ConnectionString"] ?? string.Empty;

        services.AddHealthChecks()
            .AddSqlServer(
                connectionString: connectionString,
                name:             "azure-sql",
                failureStatus:    HealthStatus.Unhealthy,
                tags:             ["db", "sql", "azure"]);

        return services;
    }
}
