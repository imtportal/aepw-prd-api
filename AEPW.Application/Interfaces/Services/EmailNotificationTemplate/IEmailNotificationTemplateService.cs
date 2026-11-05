using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.EmailNotificationTemplate;

namespace AEPW.Application.Interfaces.Services.EmailNotificationTemplate;

public interface IEmailNotificationTemplateService
{
    Task<ApiResponse<List<EmailNotificationInterpolationKeywordDto>>>
        GetAllInterpolationKeywordsAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<List<EmailNotificationTemplateResponse>>>
        GetAllEmailNotificationTemplatesAsync(CancellationToken cancellationToken = default);

    Task<ApiResponse<EmailNotificationTemplateResponse>> SaveEmailNotificationTemplateAsync(
        EmailNotificationTemplateSaveRequestDto request,
        CancellationToken cancellationToken = default);
}
