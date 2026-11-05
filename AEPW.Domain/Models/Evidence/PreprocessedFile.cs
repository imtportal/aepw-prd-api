namespace AEPW.Domain.Models.Evidence;

/// <summary>
/// The file-preprocessing classification outcome. Every file must land in exactly one of
/// these three buckets - there is no "fall through and let Foundry try anyway" path.
/// </summary>
public enum PreprocessOutcome
{
    /// <summary>xlsx/docx/pptx extracted to text/JSON in-app; send as a text input.</summary>
    ConvertedText,

    /// <summary>pdf/image - Foundry reads it natively; send as a file/image input, unchanged.</summary>
    Passthrough,

    /// <summary>Neither convertible nor Foundry-native (or extraction failed) - reject before calling Foundry.</summary>
    Unsupported
}

/// <summary>
/// Result of <c>IFilePreprocessor.Process</c>: the file's classification, plus the extracted
/// text/JSON (when ConvertedText) or the rejection reason (when Unsupported).
/// </summary>
public sealed record PreprocessedFile(
    PreprocessOutcome Outcome,
    string? Text,
    string MimeType,
    string FileName,
    string? RejectReason);
