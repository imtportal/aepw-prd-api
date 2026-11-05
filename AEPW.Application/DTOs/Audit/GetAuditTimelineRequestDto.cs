namespace AEPW.Application.DTOs.Audit;

public sealed class GetAuditTimelineRequestDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? UserId { get; set; }
    public AuditModuleType Module { get; set; } = AuditModuleType.UserManagement;
    public long? ProjectId { get; set; }
    public long? ProgramId { get; set; }
    public string? SearchText { get; set; }
    public string? EventAction { get; set; }
    public string? MetricCode { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
