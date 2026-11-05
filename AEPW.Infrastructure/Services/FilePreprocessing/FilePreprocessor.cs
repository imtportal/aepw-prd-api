using AEPW.Application.Interfaces.Services.FilePreprocessing;
using AEPW.Domain.Models.Evidence;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging;
using A = DocumentFormat.OpenXml.Drawing;
using P = DocumentFormat.OpenXml.Presentation;
using W = DocumentFormat.OpenXml.Wordprocessing;

namespace AEPW.Infrastructure.Services.FilePreprocessing;

/// <summary>
/// Cross-platform (DocumentFormat.OpenXml runs on Linux) extraction of docx/pptx content to
/// text. Stateless - safe as a singleton. xlsx/xlsm are NOT handled here - they route to
/// IExcelAgentService instead (see EvidenceProcessingService's routing); if one somehow
/// reaches this class anyway, it falls through to Unsupported rather than being converted.
/// </summary>
public sealed class FilePreprocessor : IFilePreprocessor
{
    private readonly ILogger<FilePreprocessor> _logger;

    public FilePreprocessor(ILogger<FilePreprocessor> logger)
    {
        _logger = logger;
    }

    private static readonly HashSet<string> PassthroughExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif", ".webp"
    };

    public PreprocessedFile Process(Stream fileStream, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        try
        {
            switch (extension)
            {
                case ".docx":
                    return new PreprocessedFile(PreprocessOutcome.ConvertedText, ExtractDocx(fileStream), string.Empty, fileName, null);
                case ".pptx":
                    return new PreprocessedFile(PreprocessOutcome.ConvertedText, ExtractPptx(fileStream), string.Empty, fileName, null);
                default:
                    return PassthroughExtensions.Contains(extension)
                        ? new PreprocessedFile(PreprocessOutcome.Passthrough, null, string.Empty, fileName, null)
                        : new PreprocessedFile(PreprocessOutcome.Unsupported, null, string.Empty, fileName, $"Unsupported file type: {extension}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "[FILE-PREPROCESSOR] Failed to read '{FileName}' as {Extension}; rejecting (no Foundry call).",
                fileName,
                extension);

            return new PreprocessedFile(PreprocessOutcome.Unsupported, null, string.Empty, fileName, $"Failed to read {extension} file");
        }
        finally
        {
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }
        }
    }

    /// <summary>Concatenates all paragraph texts in document order, one paragraph per line.</summary>
    private static string ExtractDocx(Stream fileStream)
    {
        using var wordDocument = WordprocessingDocument.Open(fileStream, false);
        var body = wordDocument.MainDocumentPart?.Document?.Body;

        if (body is null)
        {
            return string.Empty;
        }

        var paragraphs = body.Descendants<W.Paragraph>().Select(p => p.InnerText);

        return string.Join("\n", paragraphs);
    }

    /// <summary>Extracts all text runs per slide, in slide order, each prefixed with "=== Slide N ===".</summary>
    private static string ExtractPptx(Stream fileStream)
    {
        using var presentationDocument = PresentationDocument.Open(fileStream, false);
        var presentationPart = presentationDocument.PresentationPart;
        var slideIdList = presentationPart?.Presentation.SlideIdList;

        if (presentationPart is null || slideIdList is null)
        {
            return string.Empty;
        }

        var slideTexts = new List<string>();
        var slideNumber = 0;

        foreach (var slideId in slideIdList.Elements<P.SlideId>())
        {
            slideNumber++;

            var relationshipId = slideId.RelationshipId?.Value;
            if (relationshipId is null || presentationPart.GetPartById(relationshipId) is not SlidePart slidePart)
            {
                continue;
            }

            var slideText = string.Join("\n", slidePart.Slide.Descendants<A.Text>().Select(t => t.Text));

            slideTexts.Add($"=== Slide {slideNumber} ===\n{slideText}");
        }

        return string.Join("\n\n", slideTexts);
    }
}
