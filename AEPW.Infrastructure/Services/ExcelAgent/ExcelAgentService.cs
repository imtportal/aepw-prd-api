// OpenAI Responses types (ResponseItem, ResponseContentPart, etc.) are experimental
// (OPENAI001) as of the version this project references - same suppression as
// FoundryAgentService, which uses the identical API surface.
#pragma warning disable OPENAI001

using AEPW.Application.Interfaces.Services.ExcelAgent;
using AEPW.Domain.Models.Evidence;
using AEPW.Infrastructure.Configurations;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Responses;

namespace AEPW.Infrastructure.Services.ExcelAgent;

/// <summary>
/// Calls the "aepw-unsupported-file-processor" Foundry agent (confirmed live, version 2:
/// model gpt-5.1, CodeInterpreterTool with automatic container management) - sends the raw
/// xlsx/xlsm bytes directly, exactly like FoundryAgentService sends a PDF. Verified
/// end-to-end against the live agent: CreateInputFilePart is sufficient: no OpenAIFileClient
/// upload or manual container.FileIds wiring is needed, since the agent's automatic
/// container picks up the file attached to the input message.
/// The injected client is keyed "ExcelAgent" - constructed with NetworkTimeout/RetryPolicy
/// from ExcelAgentSettings (see ServiceCollectionExtensions), separate from FoundryAgent's.
/// </summary>
public sealed class ExcelAgentService : IExcelAgentService
{
    private readonly AIProjectClient _projectClient;
    private readonly ExcelAgentSettings _settings;
    private readonly ILogger<ExcelAgentService> _logger;

    public ExcelAgentService(
        [FromKeyedServices("ExcelAgent")] AIProjectClient projectClient,
        IOptions<ExcelAgentSettings> settings,
        ILogger<ExcelAgentService> logger)
    {
        _projectClient = projectClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<FoundryResult> ProcessAsync(
        Stream file,
        string fileName,
        string topLevelMetric,
        string l1Metric,
        string evidenceType,
        int reportingYear,
        string reportingQuarter,
        CancellationToken cancellationToken = default)
    {
        var agent = new AgentReference(_settings.AgentName, _settings.AgentVersion);

        var responsesClient = _projectClient.ProjectOpenAIClient.GetProjectResponsesClientForAgent(
            agent,
            defaultConversationId: null);

        // KB rows are keyed by (Top Level Metric + Evidence Type) only - one row per TLM, each
        // referencing a single representative L1 as metadata. Sending the evidence's own L1 here
        // made the agent reject any OTHER L1 under the same TLM ("no matching KB row"), even
        // though the correct TLM-level row exists. l1Metric is kept as a parameter for
        // diagnostics only (see log below) - it must never reach the agent payload/KB query.
        var promptText =
            $"Top Level Metric: {topLevelMetric}\n" +
            $"Evidence Type: {evidenceType}\n" +
            $"reportingYear: {reportingYear}\n" +
            $"reportingQuarter: {reportingQuarter}";

        var fileBytes = await BinaryData.FromStreamAsync(file, cancellationToken);
        var mimeType = ResolveExcelMimeType(fileName);

        var contentParts = new List<ResponseContentPart>
        {
            ResponseContentPart.CreateInputTextPart(promptText),
            ResponseContentPart.CreateInputFilePart(fileBytes, mimeType, fileName)
        };

        var inputItem = ResponseItem.CreateUserMessageItem(contentParts);

        _logger.LogInformation(
            "[EXCEL-AGENT] Calling '{AgentName}' v{AgentVersion} for '{FileName}'. KB matching uses TopLevelMetric='{TopLevelMetric}' + EvidenceType='{EvidenceType}' only - L1Metric='{L1Metric}' is diagnostic-only and is NOT sent to the agent.",
            _settings.AgentName,
            _settings.AgentVersion,
            fileName,
            topLevelMetric,
            evidenceType,
            l1Metric);

        var response = await responsesClient.CreateResponseAsync(
            new[] { inputItem },
            previousResponseId: null,
            cancellationToken);

        var outputText = response.Value.GetOutputText();

        Console.WriteLine($"[EXCEL-AGENT] Output: {outputText}");

        return new FoundryResult(outputText)
        {
            AgentIdentifier = $"{_settings.AgentName}:{_settings.AgentVersion}"
        };
    }

    private static string ResolveExcelMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        return extension switch
        {
            ".xlsm" => "application/vnd.ms-excel.sheet.macroEnabled.12",
            _ => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
    }
}
