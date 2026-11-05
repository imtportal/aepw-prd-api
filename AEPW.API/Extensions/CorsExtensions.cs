using AEPW.Domain.Constants;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace AEPW.API.Extensions;

public static class CorsExtensions
{
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(AppConstants.DefaultCorsPolicyName, builder =>
            {
                if (allowedOrigins.Length == 0)
                {
                // Development fallback — lock down in production via appsettings
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .WithExposedHeaders("Content-Disposition");
                }
                else
                {
                builder.WithOrigins(allowedOrigins)
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials()
                       .WithExposedHeaders("Content-Disposition");
                     }
            });
        });

        return services;
    }
}
