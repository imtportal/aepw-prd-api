using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.EmailNotificationTemplate;
using AEPW.Application.Interfaces.Repositories.EmailNotificationTemplate;
using AEPW.Application.Interfaces.Services.EmailNotificationTemplate;
using AEPW.Domain.Models.EmailNotificationTemplate;

namespace AEPW.Application.Services.EmailNotificationTemplate;

public sealed class EmailNotificationTemplateService
    : IEmailNotificationTemplateService
{
    private readonly IEmailNotificationTemplateRepository _repository;

    public EmailNotificationTemplateService(
        IEmailNotificationTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<List<EmailNotificationInterpolationKeywordDto>>>
        GetAllInterpolationKeywordsAsync(CancellationToken cancellationToken = default)
    {
        var keywords = await _repository.GetAllInterpolationKeywordsAsync(
            cancellationToken);

        var data = keywords
            .Select(keyword => new EmailNotificationInterpolationKeywordDto(
                keyword.EmailNotificationInterpolationKeywordId,
                keyword.Keyword,
                keyword.KeywordType,
                keyword.Description))
            .ToList();

        return ApiResponse<List<EmailNotificationInterpolationKeywordDto>>.Ok(data);
    }

    public async Task<ApiResponse<List<EmailNotificationTemplateResponse>>>
        GetAllEmailNotificationTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = await _repository.GetAllEmailNotificationTemplatesAsync(
            cancellationToken);

        var data = templates
            .Select(template => new EmailNotificationTemplateResponse(
                template.EmailNotificationTemplateId,
                template.TemplateCode,
                template.TemplateName,
                template.SubjectTemplate,
                template.BodyTemplate,
                template.RecipientTo,
                template.RecipientCc,
                template.IsActive))
            .ToList();

        return ApiResponse<List<EmailNotificationTemplateResponse>>.Ok(data);
    }

    public async Task<ApiResponse<EmailNotificationTemplateResponse>>
        SaveEmailNotificationTemplateAsync(
            EmailNotificationTemplateSaveRequestDto request,
            CancellationToken cancellationToken = default)
    {
        var template = new EmailNotificationTemplateModel
        {
            EmailNotificationTemplateId = request.EmailNotificationTemplateId,
            TemplateCode = request.TemplateCode,
            TemplateName = request.TemplateName,
            SubjectTemplate = request.SubjectTemplate,
            BodyTemplate = request.BodyTemplate,
            RecipientTo = request.RecipientTo,
            RecipientCc = request.RecipientCc,
            IsActive = request.IsActive,
            UpdatedBy = request.UpdatedBy
        };

        await _repository.SaveEmailNotificationTemplateAsync(
            template,
            cancellationToken);

        var response = new EmailNotificationTemplateResponse(
            template.EmailNotificationTemplateId,
            template.TemplateCode,
            template.TemplateName,
            template.SubjectTemplate,
            template.BodyTemplate,
            template.RecipientTo,
            template.RecipientCc,
            template.IsActive);

        return ApiResponse<EmailNotificationTemplateResponse>.Ok(
            response,
            "Email notification template saved successfully.");
    }
}
