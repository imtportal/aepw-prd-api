namespace AEPW.Application.DTOs.EmailNotificationTemplate;

/// <summary>Represents an interpolation keyword available to email templates.</summary>
public sealed record EmailNotificationInterpolationKeywordDto(
    long EmailNotificationInterpolationKeywordId,
    string Keyword,
    string KeywordType,
    string? Description);

/// <summary>Represents an active email notification template.</summary>
public sealed record EmailNotificationTemplateResponse(
    long EmailNotificationTemplateId,
    string TemplateCode,
    string TemplateName,
    string SubjectTemplate,
    string BodyTemplate,
    string RecipientTo,
    string? RecipientCc,
    bool IsActive);
