namespace AEPW.Domain.Models.Evidence;

/// <summary>
/// Strongly-typed binding for the "EvidenceProcessing" configuration section.
/// Bind via: services.Configure&lt;EvidenceProcessingSettings&gt;(configuration.GetSection(EvidenceProcessingSettings.SectionName))
/// </summary>
public sealed class EvidenceProcessingSettings
{
    public const string SectionName = "EvidenceProcessing";

    /// <summary>AppUser id attributed as CreatedBy/UpdatedBy for writes made by the processing pipeline.</summary>
    public long EvidenceServiceUserId { get; set; }
}
