using AEPW.Domain.Models.EmailNotificationTemplate;

namespace AEPW.Application.Interfaces.Repositories.EmailNotificationTemplate;

public interface IEmailNotificationTemplateRepository
{
    Task<IReadOnlyList<EmailNotificationInterpolationKeywordModel>>
        GetAllInterpolationKeywordsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EmailNotificationTemplateModel>>
        GetAllEmailNotificationTemplatesAsync(CancellationToken cancellationToken = default);

    Task SaveEmailNotificationTemplateAsync(
        EmailNotificationTemplateModel template,
        CancellationToken cancellationToken = default);
}
