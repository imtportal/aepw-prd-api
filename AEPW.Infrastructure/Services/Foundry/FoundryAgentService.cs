// The OpenAI Responses types used here (ResponseItem, ResponseContentPart, etc.) are
// marked experimental (OPENAI001) by the OpenAI SDK as of the version this project
// references. Suppressed deliberately - see delivery notes for what still needs
// verifying against the live agent.
#pragma warning disable OPENAI001

using AEPW.Application.Interfaces.Services.Foundry;
using AEPW.Domain.Models.Evidence;
using AEPW.Infrastructure.Configurations;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Responses;

namespace AEPW.Infrastructure.Services.Foundry;

/// <summary>
/// Wraps a single, stateless Responses API call to a Microsoft Foundry Agent
/// (<see cref="AIProjectClient"/> from Azure.AI.Projects) that performs extraction and
/// validation on a document in one shot. No conversation/session is created - each call
/// is independent, matching this endpoint's single-shot (no multi-turn) use case.
/// The injected client is keyed "FoundryAgent" - constructed with NetworkTimeout/RetryPolicy
/// from FoundryAgentSettings (see ServiceCollectionExtensions), separate from ExcelAgent's.
/// </summary>
public sealed class FoundryAgentService : IFoundryAgentService
{
    private readonly AIProjectClient _projectClient;
    private readonly FoundryAgentSettings _settings;
    private readonly ILogger<FoundryAgentService> _logger;

    public FoundryAgentService(
        [FromKeyedServices("FoundryAgent")] AIProjectClient projectClient,
        IOptions<FoundryAgentSettings> settings,
        ILogger<FoundryAgentService> logger)
    {
        _projectClient = projectClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<FoundryResult> ProcessAsync(
        Stream file,
        string fileName,
        string mimeType,
        string metricCode,
        string evidenceType,
        int reportingYear,
        string reportingQuarter,
        PreprocessedFile preprocessedFile,
        CancellationToken cancellationToken = default)
    {
        if (preprocessedFile.Outcome == PreprocessOutcome.Unsupported)
        {
            throw new InvalidOperationException(
                $"FoundryAgentService.ProcessAsync received an Unsupported preprocessed file ('{preprocessedFile.FileName}') - the caller must reject unsupported files before calling Foundry.");
        }

        var agent = new AgentReference(_settings.AgentName, _settings.AgentVersion);

        var responsesClient = _projectClient.ProjectOpenAIClient.GetProjectResponsesClientForAgent(
            agent,
            defaultConversationId: null);

        var promptText =
            $"MetricCode: {metricCode}\n" +
            $"EvidenceType: {evidenceType}\n" +
            $"ReportingYear: {reportingYear}\n" +
            $"ReportingQuarter: {reportingQuarter}";

        var contentParts = new List<ResponseContentPart>
        {
            ResponseContentPart.CreateInputTextPart(promptText)
        };

        if (preprocessedFile.Outcome == PreprocessOutcome.ConvertedText)
        {
            _logger.LogInformation(
                "[FOUNDRY-AGENT] Sending preprocessed text/JSON for '{FileName}' ({Length} chars) as a text input.",
                fileName,
                preprocessedFile.Text?.Length ?? 0);

            var extractedContentPrompt =
                $"The following is the extracted content of an Office file ({fileName}). Treat it as the document content for extraction:\n\n" +
                preprocessedFile.Text;

            contentParts.Add(ResponseContentPart.CreateInputTextPart(extractedContentPrompt));
        }
        else
        {
            var fileBytes = await BinaryData.FromStreamAsync(file, cancellationToken);
            var resolvedMimeType = ResolveMimeType(fileName, mimeType);

            contentParts.Add(BuildInputPart(fileBytes, resolvedMimeType, fileName));
        }

        var inputItem = ResponseItem.CreateUserMessageItem(contentParts);

        var response = await responsesClient.CreateResponseAsync(
            new[] { inputItem },
            previousResponseId: null,
            cancellationToken);

        var outputText = response.Value.GetOutputText();

        Console.WriteLine($"[FOUNDRY-AGENT] Output: {outputText}");

        return new FoundryResult(outputText)
        {
            AgentIdentifier = $"{_settings.AgentName}:{_settings.AgentVersion}"
        };
    }

    /// <summary>
    /// Images must be sent as an input IMAGE content part (base64 data URL) - the Responses
    /// API rejects images sent via CreateInputFilePart with "unsupported MIME type". PDFs
    /// (and anything else) keep using CreateInputFilePart as before.
    /// </summary>
    private static ResponseContentPart BuildInputPart(BinaryData fileBytes, string mimeType, string fileName)
    {
        if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            var dataUrl = $"data:{mimeType};base64,{Convert.ToBase64String(fileBytes.ToArray())}";
            return ResponseContentPart.CreateInputImagePart(new Uri(dataUrl), imageDetailLevel: null);
        }

        return ResponseContentPart.CreateInputFilePart(fileBytes, mimeType, fileName);
    }

    private static readonly Dictionary<string, string> ExtensionMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".tif"] = "image/tiff",
        [".tiff"] = "image/tiff",
        [".bmp"] = "image/bmp",
        [".gif"] = "image/gif"
    };

    /// <summary>
    /// Derives the MIME type from the file extension, since the incoming Content-Type
    /// (from the Worker's multipart upload) is frequently "application/octet-stream" and
    /// Foundry rejects that. Falls back to a real, specific incoming mimeType if the
    /// extension isn't recognized, then to "application/pdf" as a last resort.
    /// </summary>
    private static string ResolveMimeType(string fileName, string? mimeType)
    {
        var extension = Path.GetExtension(fileName);

        if (!string.IsNullOrEmpty(extension) && ExtensionMimeTypes.TryGetValue(extension, out var extensionMimeType))
        {
            return extensionMimeType;
        }

        if (!string.IsNullOrWhiteSpace(mimeType) &&
            !mimeType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return mimeType;
        }

        return "application/pdf";
    }
}
