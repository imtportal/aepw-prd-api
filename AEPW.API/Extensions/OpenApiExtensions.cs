using Asp.Versioning;
using AEPW.Domain.Constants;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace AEPW.API.Extensions;

public static class OpenApiExtensions
{
    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        // ── API versioning ───────────────────────────────────────────────────
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion                   = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions                   = true;
            options.ApiVersionReader                    = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-Api-Version"));
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat           = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // ── Native .NET 10 OpenAPI document (consumed by Scalar + Swagger) ──
        services.AddOpenApi(AppConstants.ApiVersionV1, options =>
        {
            options.AddDocumentTransformer((document, context, ct) =>
            {
                document.Info = new()
                {
                    Title       = AppConstants.ApiTitle,
                    Version     = AppConstants.ApiVersionV1,
                    Description = "Enterprise-grade ASP.NET Core Web API built on .NET 10.\n\n" +
                                  "## Features\n" +
                                  "- Azure SQL via ADO.NET + Stored Procedures\n" +
                                  "- Soft-delete pattern\n" +
                                  "- Standard `ApiResponse<T>` envelope on every response\n" +
                                  "- `X-Correlation-Id` header propagated on all requests",
                    Contact = new()
                    {
                        Name = "AEPW Team"
                    }
                };
                return Task.CompletedTask;
            });
        });

        // ── Swashbuckle (classic Swagger UI at /swagger) ─────────────────────
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(AppConstants.ApiVersionV1, new OpenApiInfo
            {
                Title       = AppConstants.ApiTitle,
                Version     = AppConstants.ApiVersionV1,
                Description = "Enterprise-grade ASP.NET Core Web API — .NET 10\n\n" +
                              "Use the `X-Correlation-Id` header to trace requests end-to-end.",
                Contact = new OpenApiContact
                {
                    Name = "AEPW Team"
                }
            });

            // Include XML doc comments in Swagger UI
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);

            // X-Correlation-Id header visible in Swagger UI
            options.AddSecurityDefinition("CorrelationId", new OpenApiSecurityScheme
            {
                Name        = "X-Correlation-Id",
                In          = ParameterLocation.Header,
                Type        = SecuritySchemeType.ApiKey,
                Description = "Optional correlation ID for end-to-end request tracing."
            });

            // Resolve version conflicts when multiple API versions are present
            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                if (!apiDesc.TryGetMethodInfo(out var methodInfo)) return false;
                var versions = methodInfo.DeclaringType?
                    .GetCustomAttributes(true)
                    .OfType<Asp.Versioning.ApiVersionAttribute>()
                    .SelectMany(a => a.Versions);
                return versions?.Any(v => $"v{v}" == docName) ?? false;
            });
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint(
                $"/swagger/{AppConstants.ApiVersionV1}/swagger.json",
                $"{AppConstants.ApiTitle} {AppConstants.ApiVersionV1}");

            options.RoutePrefix = "swagger";                       // UI lives at /swagger
            options.DocumentTitle = AppConstants.ApiTitle;
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.EnableTryItOutByDefault();
            options.DefaultModelsExpandDepth(-1);                  // collapse schemas by default
        });

        return app;
    }
}
