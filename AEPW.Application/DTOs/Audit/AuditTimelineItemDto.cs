namespace AEPW.Application.DTOs.Audit;

public sealed class AuditTimelineItemDto
{
    public string EntityName { get; set; } = string.Empty;
    public string? RecordTitle { get; set; }
    public DateTime ChangedOn { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string? EventAction { get; set; }
    /// <summary>Companion comment (e.g. Approval or Rejection comment) associated with a Status change event.</summary>
    public string? Comment { get; set; }
}

