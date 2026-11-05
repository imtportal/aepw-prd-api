using AEPW.Domain.Models.Evidence;

namespace AEPW.Application.Interfaces.Services.FilePreprocessing;

/// <summary>
/// Extracts the data-bearing content of docx/pptx to text so it can be sent to the Foundry
/// agent as a TEXT input - the agent cannot read these formats directly. PDFs, images, and
/// anything else pass through unchanged. xlsx/xlsm are NOT handled here - they route to
/// IExcelAgentService instead (see EvidenceProcessingService's routing).
/// </summary>
public interface IFilePreprocessor
{
    /// <summary>
    /// Dispatches on <paramref name="fileName"/>'s extension. Never throws - extraction
    /// failures fall back to Unsupported (reject - no Foundry call for that file).
    /// </summary>
    /// <param name="fileStream">
    /// The file content. Repositioned to the start before returning, regardless of outcome,
    /// so the caller can still read it from the beginning for the passthrough path.
    /// </param>
    PreprocessedFile Process(Stream fileStream, string fileName);
}
