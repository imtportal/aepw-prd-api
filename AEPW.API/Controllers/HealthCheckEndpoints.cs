using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace AEPW.API.Controllers;

/// <summary>
/// Exposes /health and /health/ready endpoints consumed by Azure App Service
/// health probes and load balancers.
/// </summary>
public static class HealthCheckEndpoints
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public static IEndpointRouteBuilder MapHealthCheckEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Liveness: is the process alive?
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate      = _ => false,   // no checks — just confirms process is up
            ResponseWriter = WriteJsonResponse
        });

        // Readiness: can the service serve traffic (DB reachable, etc.)?
        endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate      = _ => true,
            ResponseWriter = WriteJsonResponse
        });

        return endpoints;
    }

    private static async Task WriteJsonResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var result = new
        {
            status  = report.Status.ToString(),
            checks  = report.Entries.Select(e => new
            {
                name        = e.Key,
                status      = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration    = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(result, _jsonOptions));
    }
}
