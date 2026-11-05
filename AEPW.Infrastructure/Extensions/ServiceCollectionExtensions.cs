using AEPW.Application.Interfaces.Repositories;
using AEPW.Application.Interfaces.Repositories.Authorization;
using AEPW.Application.Interfaces.Repositories.BusinessRoleMapping;
using AEPW.Application.Interfaces.Repositories.FundingRecipient;
using AEPW.Application.Interfaces.Repositories.ImpactMaster;
using AEPW.Application.Interfaces.Repositories.EmailNotificationTemplate;
using AEPW.Application.Interfaces.Repositories.Logging;
using AEPW.Application.Interfaces.Repositories.Lookup;
using AEPW.Application.Interfaces.Repositories.Metric;
using AEPW.Application.Interfaces.Repositories.Program;
using AEPW.Application.Interfaces.Repositories.Review;
using AEPW.Application.Interfaces.Repositories.ReviewTask;
using AEPW.Application.Interfaces.Repositories.User;
using AEPW.Application.Interfaces.Services;
using AEPW.Application.Interfaces.Services.Authorization;
using AEPW.Application.Interfaces.Services.AzureAd;
using AEPW.Application.Interfaces.Services.AzureBlobService;
using AEPW.Application.Interfaces.Services.BusinessRoleMapping;
using AEPW.Application.Interfaces.Services.ExcelImportExportService;
using AEPW.Application.Interfaces.Services.ExcelParser;
using AEPW.Application.Interfaces.Services.FundingRecipient;
using AEPW.Application.Interfaces.Services.ImpactMaster;
using AEPW.Application.Interfaces.Services.EmailNotificationTemplate;
using AEPW.Application.Interfaces.Services.Logging;
using AEPW.Application.Interfaces.Services.Lookup;
using AEPW.Application.Interfaces.Services.Metric;
using AEPW.Application.Interfaces.Services.Notification;
using AEPW.Application.Interfaces.Services.Program;
using AEPW.Application.Interfaces.Services.Review;
using AEPW.Application.Interfaces.Services.ReviewTask;
using AEPW.Application.Interfaces.Services.SharepointService;
using AEPW.Application.Interfaces.Services.User;
using AEPW.Application.Services;
using AEPW.Application.Services.Authorization;
using AEPW.Application.Services.AzureAd;
using AEPW.Application.Services.AzureBlobService;
using AEPW.Application.Services.BusinessRoleMapping;
using AEPW.Application.Services.ExcelParser;
using AEPW.Application.Interfaces.Repositories.Dashboard;
using AEPW.Application.Interfaces.Services.Dashboard;
using AEPW.Application.Services.Dashboard;
using AEPW.Application.Interfaces.Repositories.Evidence;
using AEPW.Application.Interfaces.Services.Evidence;
using AEPW.Application.Interfaces.Services.ExcelAgent;
using AEPW.Application.Interfaces.Services.ExchangeRate;
using AEPW.Application.Interfaces.Services.FilePreprocessing;
using AEPW.Application.Interfaces.Services.Foundry;
using AEPW.Application.Services.Evidence;
using AEPW.Infrastructure.Repositories.Evidence;
using AEPW.Infrastructure.Services.ExcelAgent;
using AEPW.Infrastructure.Services.ExchangeRate;
using AEPW.Infrastructure.Services.FilePreprocessing;
using AEPW.Infrastructure.Services.Foundry;
using AEPW.Domain.Models.Evidence;
using Azure.AI.Projects;
using Azure.Identity;
using AEPW.Application.Services.Review;
using AEPW.Application.Interfaces.Repositories.Report;
using AEPW.Application.Interfaces.Services.Report;

//
using AEPW.Application.Services.FundingRecipient;
using AEPW.Application.Services.ImpactMaster;
using AEPW.Application.Services.EmailNotificationTemplate;
using AEPW.Application.Services.Logging;
using AEPW.Application.Services.Lookup;
using AEPW.Application.Services.Metric;
using AEPW.Application.Services.Notification;
using AEPW.Application.Services.Program;
using AEPW.Application.Services.ReviewTask;
using AEPW.Application.Services.Services;
using AEPW.Application.Services.Sharepoint;
using AEPW.Application.Services.User;
using AEPW.Infrastructure.Configurations;
using AEPW.Infrastructure.Data;
using AEPW.Infrastructure.Repositories.Authorization;
using AEPW.Infrastructure.Repositories.BusinessRoleMapping;
using AEPW.Infrastructure.Repositories.FundingRecipient;
using AEPW.Infrastructure.Repositories.ImpactMaster;
using AEPW.Infrastructure.Repositories.EmailNotificationTemplate;
using AEPW.Infrastructure.Repositories.Logging;
using AEPW.Infrastructure.Repositories.Lookup;
using AEPW.Infrastructure.Repositories.Metric;
using AEPW.Infrastructure.Repositories.Program;
using AEPW.Infrastructure.Repositories.Review;
using AEPW.Infrastructure.Repositories.ReviewTask;
using AEPW.Infrastructure.Repositories.User;
using AEPW.Infrastructure.Repositories.Report;


using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph.Models;
using System.ClientModel.Primitives;
using AEPW.Application.Services.Report;

namespace AEPW.Infrastructure.Extensions;




/// IServiceCollection extension that registers all Infrastructure and Application

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Strongly-typed settings ──────────────────────────────────────────
        services.Configure<DatabaseSettings>(
            configuration.GetSection(DatabaseSettings.SectionName));
        services.Configure<FoundryAgentSettings>(
            configuration.GetSection(FoundryAgentSettings.SectionName));
        services.Configure<EvidenceProcessingSettings>(
            configuration.GetSection(EvidenceProcessingSettings.SectionName));
        services.Configure<FrankfurterSettings>(
            configuration.GetSection(FrankfurterSettings.SectionName));
        services.Configure<ExcelAgentSettings>(
            configuration.GetSection(ExcelAgentSettings.SectionName));

        // ── Data access ──────────────────────────────────────────────────────
        services.AddScoped<ISqlConnectionFactory, SqlConnectionFactory>();

        // ── Repositories ─────────────────────────────────────────────────────
        services.AddScoped<IAuthorizationUserRepository, AuthorizationRepository>();
        services.AddScoped<IRoleAccessRepository, RoleAccessRepository>();
        services.AddScoped<IMetricRepository, MetricRepository>();
        services.AddScoped<ILookupRepository, LookupRepository>();
        services.AddScoped<IImpactMasterRepository, ImpactMasterRepository>();
        services.AddScoped<IEmailNotificationTemplateRepository, EmailNotificationTemplateRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IBusinessRoleMappingRepository, BusinessRoleMappingRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IImpactMasterRepository, ImpactMasterRepository>();
        services.AddScoped<IDataCollectionExcelGeneratorService, DataCollectionExcelGeneratorService>();
        services.AddScoped<IExceptionLogRepository, ExceptionLogRepository>();
        services.AddScoped<IFundingRecipientRepository, FundingRecipientRepository>();
        services.AddScoped<IProgramRepository, ProgramRepository>();
    services.AddScoped<IReviewTaskRepository, ReviewTaskRepository>();
    services.AddScoped<IDashboardRepository, DashboardRepository>();
    services.AddScoped<IEvidenceProcessingRepository, EvidenceProcessingRepository>();
    services.AddScoped<IEvidenceReviewRepository, EvidenceReviewRepository>();
    services.AddScoped<IReportRepository, ReportRepository>();
    services.AddScoped<AEPW.Application.Interfaces.Repositories.Audit.IAuditRepository, AEPW.Infrastructure.Repositories.Audit.AuditRepository>();

    // ── Services ─────────────────────────────────────────────────────────        services.AddScoped<IAuthorizationService, AuthorizationService>();        services.AddScoped<IMetricService, MetricService>();
    services.AddScoped<IMetricService, MetricService>();
        services.AddScoped<ILookupService, LookupService>();
        services.AddScoped<IImpactMasterService, ImpactMasterService>();
        services.AddScoped<IEmailNotificationTemplateService, EmailNotificationTemplateService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBusinessRoleMappingService, BusinessRoleMappingService>();
        services.AddScoped<IAzureAdService, AzureAdService>();
        services.AddScoped<IImpactMasterService, ImpactMasterService>();
        services.AddScoped<ISharepointService, SharepointService>();
        services.AddScoped<IAuthorizationUserService, AuthorizationService>();
        services.AddScoped<IRoleAccessService, RoleAccessService>();
        services.AddScoped<IExcelParserService, ExcelParserService>();
        services.AddScoped<IFundingRecipientService, FundingRecipientService>();
        services.AddScoped<IReviewTaskService, ReviewTaskService>();
        services.AddScoped<IProgramService, ProgramService>();
        services.AddScoped<IAzureBlobService, AzureBlobService>();
    services.AddScoped<IDashboardService, DashboardService>();
    services.AddScoped<IReportService, ReportService>();
    services.AddScoped<SummaryExcelGeneratorService>();
    services.AddScoped<ReviewMetricTrackerExcelGeneratorService>();

    services.AddScoped<ProjectReportCpmExcelGeneratorService>();
    services.AddScoped<AnnualPortfolioExcelGeneratorService>();
    services.AddScoped<ProjectsOnlyExcelGeneratorService>();

    services.AddScoped<
        PerTopLevelMetricExcelGeneratorService>();

    services.AddScoped<AEPW.Application.Interfaces.Services.Audit.IAuditService, AEPW.Application.Services.Audit.AuditService>();

    services.AddScoped<IExceptionLogService, ExceptionLogService>();

        // ── Evidence processing (Foundry Agent pipeline) ─────────────────────
        services.AddScoped<IEvidenceProcessingService, EvidenceProcessingService>();
        services.AddScoped<IEvidenceReviewService, EvidenceReviewService>();
        services.AddScoped<IFoundryAgentService, FoundryAgentService>();
        services.AddScoped<IExcelAgentService, ExcelAgentService>();
        services.AddSingleton<IFilePreprocessor, FilePreprocessor>();

        services.AddHttpClient<IExchangeRateService, FrankfurterExchangeRateService>((sp, client) =>
        {
            var frankfurterSettings = sp.GetRequiredService<IOptions<FrankfurterSettings>>().Value;

            client.BaseAddress = new Uri(frankfurterSettings.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(frankfurterSettings.TimeoutSeconds);
        });

        // ── Foundry AIProjectClient instances ────────────────────────────────
        // Keyed separately (not one shared singleton) because FoundryAgent and ExcelAgent
        // need DIFFERENT NetworkTimeout/retry budgets - see FoundryAgentSettings/
        // ExcelAgentSettings.NetworkTimeoutSeconds/MaxRetries. Both point at the same
        // Foundry project endpoint (FoundryAgentSettings.Endpoint); only the client options
        // differ. GetProjectResponsesClientForAgent takes no per-call options, so this
        // construction-time NetworkTimeout/RetryPolicy is the only place to set them.
        services.AddKeyedSingleton<AIProjectClient>("FoundryAgent", (sp, _) =>
        {
            var settings = sp.GetRequiredService<IOptions<FoundryAgentSettings>>().Value;

            return new AIProjectClient(
                new Uri(settings.Endpoint),
                new DefaultAzureCredential(),
                new AIProjectClientOptions
                {
                    NetworkTimeout = TimeSpan.FromSeconds(settings.NetworkTimeoutSeconds),
                    RetryPolicy = new ClientRetryPolicy(settings.MaxRetries)
                });
        });

        services.AddKeyedSingleton<AIProjectClient>("ExcelAgent", (sp, _) =>
        {
            var foundryEndpoint = sp.GetRequiredService<IOptions<FoundryAgentSettings>>().Value.Endpoint;
            var excelSettings = sp.GetRequiredService<IOptions<ExcelAgentSettings>>().Value;

            return new AIProjectClient(
                new Uri(foundryEndpoint),
                new DefaultAzureCredential(),
                new AIProjectClientOptions
                {
                    NetworkTimeout = TimeSpan.FromSeconds(excelSettings.NetworkTimeoutSeconds),
                    RetryPolicy = new ClientRetryPolicy(excelSettings.MaxRetries)
                });
        });

        // ── Azure Service Bus ──────────────────────────────────────────────
        services.AddSingleton(_ =>
        {
            var options = new ServiceBusClientOptions
            {
                TransportType = ServiceBusTransportType.AmqpWebSockets
            };

            Console.WriteLine("ServiceBus:ConnectionString to be used service-wise", configuration["ServiceBus:ConnectionString"]);

            return new ServiceBusClient(
                configuration["ServiceBus:ConnectionString"]!,
                options);
        });
        services.AddScoped<IServiceBusService, ServiceBusService>();

        return services;
    }
}
