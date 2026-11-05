using System.ComponentModel.DataAnnotations;

namespace AEPW.Application.DTOs.EmailNotificationTemplate;

/// <summary>Fields required to create or update an email notification template.</summary>
public sealed record EmailNotificationTemplateSaveRequestDto
{
    [Range(0, long.MaxValue)]
    public long EmailNotificationTemplateId { get; init; }

    [Required, MaxLength(50)]
    public required string TemplateCode { get; init; }

    [Required, MaxLength(100)]
    public required string TemplateName { get; init; }

    [Required, MaxLength(500)]
    public required string SubjectTemplate { get; init; }

    [Required]
    public required string BodyTemplate { get; init; }

    [Required, MaxLength(500)]
    public required string RecipientTo { get; init; }

    [MaxLength(500)]
    public string? RecipientCc { get; init; }

    public bool IsActive { get; init; }

    [Range(1, long.MaxValue)]
    public long UpdatedBy { get; init; }
}
