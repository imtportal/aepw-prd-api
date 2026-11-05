namespace AEPW.Domain.Models.EmailNotificationTemplate;

public sealed class EmailNotificationInterpolationKeywordModel
{
    public long EmailNotificationInterpolationKeywordId { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public string KeywordType { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class EmailNotificationTemplateModel
{
    public long EmailNotificationTemplateId { get; set; }
    public string TemplateCode { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public string RecipientTo { get; set; } = string.Empty;
    public string? RecipientCc { get; set; }
    public bool IsActive { get; set; }
    public long UpdatedBy { get; set; }
}
