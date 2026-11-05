using AEPW.API.Extensions;
using AEPW.Domain.Constants;
using AEPW.Domain.Models.Sharepoint;
using AEPW.Infrastructure.Extensions;
using Azure.Core.Diagnostics;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Diagnostics.Tracing;
using System.Text.Json;

// This will print Azure SDK internal lifecycle logs to your console window
using var listener = AzureEventSourceListener.CreateConsoleLogger(EventLevel.Informational);

// ── Bootstrap logger ─────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    //Log.Information("Starting {ApplicationName}", AppConstants.ApiTitle);

    var builder = WebApplication.CreateBuilder(args);
    Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

    // ── Azure Key Vault overrides specific secrets on top of appsettings.json ────
    // appsettings.json / appsettings.{Environment}.json still load normally.
    // Key Vault is added last, so it wins only for the secret names that exist there;
    // every other config key keeps coming from the JSON files as before.
    var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];

    if (string.IsNullOrWhiteSpace(keyVaultUri))
    {
        throw new InvalidOperationException(
            "KeyVault:VaultUri is not set in configuration.");
    }

    var credentialOptions = new DefaultAzureCredentialOptions
    {
        // Allow developer sign-in credentials only on local Development machines.
        ExcludeVisualStudioCredential = !builder.Environment.IsDevelopment(),
        ExcludeVisualStudioCodeCredential = !builder.Environment.IsDevelopment(),
        ExcludeAzureCliCredential = !builder.Environment.IsDevelopment(),
        ExcludeAzurePowerShellCredential = !builder.Environment.IsDevelopment(),
        ExcludeInteractiveBrowserCredential = true
    };

    var keyVaultCredential = new DefaultAzureCredential(credentialOptions);

    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUri),
        keyVaultCredential);

    Console.WriteLine(
        $"KeyVault URI: {builder.Configuration["KeyVault:VaultUri"]}");

    Console.WriteLine(
        $"SharePoint TenantId exists: {!string.IsNullOrWhiteSpace(
            builder.Configuration["SharePointSettings:TenantId"])}");

    Console.WriteLine(
        $"SharePoint ClientId exists: {!string.IsNullOrWhiteSpace(
            builder.Configuration["SharePointSettings:ClientId"])}");

    Console.WriteLine(
        $"SharePoint ClientSecret exists: {!string.IsNullOrWhiteSpace(
            builder.Configuration["SharePointSettings:ClientSecret"])}");

    Console.WriteLine(
        $"SharePoint SiteUrl: {builder.Configuration["SharePointSettings:SiteUrl"]}");

    Console.WriteLine(
        $"SharePoint DocumentLibrary: {builder.Configuration["SharePointSettings:DocumentLibrary"]}");

    Console.WriteLine(
        $"SharePoint FolderPath: {builder.Configuration["SharePointSettings:FolderPath"]}");

    Console.WriteLine(
        $"DataCollectionTemplate ContainerName: {builder.Configuration["DataCollectionTemplate:ContainerName"]}");

    Console.WriteLine(
        $"DataCollectionTemplate TemplateUrl: {builder.Configuration["DataCollectionTemplate:TemplateUrl"]}");

    Console.WriteLine(
        $"DataCollectionTemplate ConnectionString exists: {!string.IsNullOrWhiteSpace(
            builder.Configuration["DataCollectionTemplate:ConnectionString"])}");

    Console.WriteLine("================================================");
    builder.Services.AddScoped<GraphServiceClient>(sp =>
    {
        var credential = new ClientSecretCredential(
            builder.Configuration["AzureAd:TenantId"],
            builder.Configuration["AzureAd:ClientId"],
            builder.Configuration["AzureAd:ClientSecret"]);

        return new GraphServiceClient(
            credential,
            new[] { "https://graph.microsoft.com/.default" });
    });

    builder.Services.Configure<SharePointSettings>(
        builder.Configuration.GetSection("SharePointSettings")
    );

    // ── Serilog from appsettings ─────────────────────────────────────────────
    // ── Serilog ──────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .Enrich.WithEnvironmentName()
              .Enrich.WithThreadId()
              .Enrich.WithProcessId());

    // ── Controllers + JSON ───────────────────────────────────────────────────
    builder.Services
        .AddControllers()
        .AddJsonOptions(opt =>
        {
            opt.JsonSerializerOptions.PropertyNamingPolicy =
                System.Text.Json.JsonNamingPolicy.CamelCase;

            opt.JsonSerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });



    // ── Infrastructure ───────────────────────────────────────────────────────
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Swagger/OpenAPI ──────────────────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // ── CORS ─────────────────────────────────────────────────────────────────
    builder.Services.AddCorsPolicy(builder.Configuration);

    // ── Health Checks ────────────────────────────────────────────────────────
    builder.Services.AddApplicationHealthChecks(builder.Configuration);

    // ── Http Context Accessor ────────────────────────────────────────────────
    builder.Services.AddHttpContextAccessor();

    // ── Azure AD Authentication ──────────────────────────────────────────────
    builder.Services
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddMicrosoftIdentityWebApi(
          jwtOptions =>
          {
              builder.Configuration.Bind("AzureAd", jwtOptions);

              jwtOptions.Events = new JwtBearerEvents
              {
                  OnAuthenticationFailed = context =>
                  {
                      Log.Warning(context.Exception,
                          "JWT authentication failed");

                      return Task.CompletedTask;
                  },

                  OnTokenValidated = context =>
                  {
                      Log.Information(
                          "JWT validated for {User}",
                          context.Principal?.Identity?.Name);

                      return Task.CompletedTask;
                  }
              };
          },
          identityOptions =>
          {
              builder.Configuration.Bind("AzureAd", identityOptions);
          });

    builder.Services.AddAuthorization();

    var app = builder.Build();

    // ── Middleware Pipeline ──────────────────────────────────────────────────

    app.UseGlobalExceptionHandler();
    app.UseCorrelationId();

    app.UseSerilogRequestLogging(opts =>
    {
        opts.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

        opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set(
                "RequestHost",
                httpContext.Request.Host.Value ?? string.Empty);

            diagnosticContext.Set(
                "RequestScheme",
                httpContext.Request.Scheme);

            diagnosticContext.Set(
                "UserAgent",
                httpContext.Request.Headers.UserAgent.ToString());
        };
    });

    app.UseHttpsRedirection();

    app.UseCors(AppConstants.DefaultCorsPolicyName);

    // Authentication MUST come before Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    // ── Swagger ──────────────────────────────────────────────────────────────
    app.UseSwagger();

    app.UseSwaggerUI();

    app.MapScalarApiReference(opts =>
    {
        opts.Title = AppConstants.ApiTitle;
        opts.DefaultHttpClient =
            new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    app.MapOpenApi();

    await app.RunAsync();

    return 0;
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(
        ex,
        "{ApplicationName} terminated unexpectedly",
        AppConstants.ApiTitle);

    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}