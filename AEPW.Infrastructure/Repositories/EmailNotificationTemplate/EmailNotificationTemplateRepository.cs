using AEPW.Application.Interfaces.Repositories.EmailNotificationTemplate;
using AEPW.Domain.Constants;
using AEPW.Domain.Models.EmailNotificationTemplate;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AEPW.Infrastructure.Repositories.EmailNotificationTemplate;

public sealed class EmailNotificationTemplateRepository
    : IEmailNotificationTemplateRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public EmailNotificationTemplateRepository(
        ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<IReadOnlyList<EmailNotificationInterpolationKeywordModel>>
        GetAllInterpolationKeywordsAsync(CancellationToken cancellationToken = default)
    {
        var keywords = new List<EmailNotificationInterpolationKeywordModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.EmailNotificationTemplate.GetAllInterpolationKeywords,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            keywords.Add(new EmailNotificationInterpolationKeywordModel
            {
                EmailNotificationInterpolationKeywordId = Convert.ToInt64(
                    reader["EmailNotificationInterpolationKeywordId"]),
                Keyword = reader["Keyword"]?.ToString() ?? string.Empty,
                KeywordType = reader["KeywordType"]?.ToString() ?? string.Empty,
                Description = reader["Description"] == DBNull.Value
                    ? null
                    : reader["Description"].ToString()
            });
        }

        return keywords;
    }

    public async Task<IReadOnlyList<EmailNotificationTemplateModel>>
        GetAllEmailNotificationTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var templates = new List<EmailNotificationTemplateModel>();

        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.EmailNotificationTemplate.GetAllEmailNotificationTemplates,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            templates.Add(new EmailNotificationTemplateModel
            {
                EmailNotificationTemplateId = Convert.ToInt64(
                    reader["EmailNotificationTemplateId"]),
                TemplateCode = reader["TemplateCode"]?.ToString() ?? string.Empty,
                TemplateName = reader["TemplateName"]?.ToString() ?? string.Empty,
                SubjectTemplate = reader["SubjectTemplate"]?.ToString() ?? string.Empty,
                BodyTemplate = reader["BodyTemplate"]?.ToString() ?? string.Empty,
                RecipientTo = reader["RecipientTo"]?.ToString() ?? string.Empty,
                RecipientCc = reader["RecipientCc"] == DBNull.Value
                    ? null
                    : reader["RecipientCc"].ToString(),
                IsActive = Convert.ToBoolean(reader["IsActive"])
            });
        }

        return templates;
    }

    public async Task SaveEmailNotificationTemplateAsync(
        EmailNotificationTemplateModel template,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            StoredProcedures.EmailNotificationTemplate.SaveEmailNotificationTemplate,
            connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter(
            "@EmailNotificationTemplateId",
            SqlDbType.BigInt)
        {
            Value = template.EmailNotificationTemplateId
        });
        command.Parameters.Add(new SqlParameter("@TemplateCode", SqlDbType.VarChar, 50)
        {
            Value = template.TemplateCode
        });
        command.Parameters.Add(new SqlParameter("@TemplateName", SqlDbType.VarChar, 100)
        {
            Value = template.TemplateName
        });
        command.Parameters.Add(new SqlParameter("@SubjectTemplate", SqlDbType.NVarChar, 500)
        {
            Value = template.SubjectTemplate
        });
        command.Parameters.Add(new SqlParameter("@BodyTemplate", SqlDbType.NVarChar, -1)
        {
            Value = template.BodyTemplate
        });
        command.Parameters.Add(new SqlParameter("@RecipientTo", SqlDbType.NVarChar, 500)
        {
            Value = template.RecipientTo
        });
        command.Parameters.Add(new SqlParameter("@RecipientCc", SqlDbType.NVarChar, 500)
        {
            Value = (object?)template.RecipientCc ?? DBNull.Value
        });
        command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit)
        {
            Value = template.IsActive
        });
        command.Parameters.Add(new SqlParameter("@UpdatedBy", SqlDbType.BigInt)
        {
            Value = template.UpdatedBy
        });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
