using AEPW.Application.Contracts.Responses;
using AEPW.Application.DTOs.AzureAdUser;
using AEPW.Application.Interfaces.Services.AzureAd;
using Microsoft.Graph;

namespace AEPW.Application.Services.AzureAd;

public sealed class AzureAdService : IAzureAdService
{
    private readonly GraphServiceClient _graph;

    public AzureAdService(GraphServiceClient graph)
    {
        _graph = graph;
    }

    public async Task<ApiResponse<List<AzureAdUserLookupDto>>>
        SearchUsersAsync(string term)
    {
        var sanitizedTerm = term?.Trim();

        if (string.IsNullOrWhiteSpace(sanitizedTerm)
            || sanitizedTerm.Length < 2)
        {
            return ApiResponse<List<AzureAdUserLookupDto>>
                .Ok(new List<AzureAdUserLookupDto>());
        }

        sanitizedTerm = sanitizedTerm.Replace("'", "''");

        var users = await _graph.Users.GetAsync(config =>
        {
            config.QueryParameters.Filter =
                $"(startsWith(displayName,'{sanitizedTerm}') or startsWith(userPrincipalName,'{sanitizedTerm}')) and accountEnabled eq true";

            config.QueryParameters.Select =
            [
                "id",
                "displayName",
                "givenName",
                "surname",
                "userPrincipalName",
                "mail",
                "jobTitle",
                "department",
                "accountEnabled",
                "mobilePhone",
                "city"
            ];

            config.QueryParameters.Top = 20;
        });

        var data = users?.Value?
            .Where(u =>
                u.AccountEnabled == true)
            .Select(u => new AzureAdUserLookupDto
            {
                Id = u.Id ?? string.Empty,
                DisplayName = u.DisplayName,
                GivenName = u.GivenName,
                Surname = u.Surname,
                UserPrincipalName = u.UserPrincipalName,
                Mail = u.Mail,
                JobTitle = u.JobTitle,
                Department = u.Department,

                MobilePhone = u.MobilePhone,
                City = u.City
            })
            .ToList()
            ?? new List<AzureAdUserLookupDto>();

        return ApiResponse<List<AzureAdUserLookupDto>>.Ok(data);
    }
    public async Task<ApiResponse<List<AzureAdUserLookupDto>>> GetUsersByEmailsAsync(
    IReadOnlyList<string> emails)
    {
        if (emails == null || emails.Count == 0)
        {
            return ApiResponse<List<AzureAdUserLookupDto>>
                .Ok(new List<AzureAdUserLookupDto>());
        }

        var users = new List<AzureAdUserLookupDto>();

        foreach (var email in emails
                     .Where(e => !string.IsNullOrWhiteSpace(e))
                     .Select(e => e.Trim())
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var user = await _graph.Users[email].GetAsync(config =>
                {
                    config.QueryParameters.Select =
                    [
                        "id",
                    "displayName",
                    "givenName",
                    "surname",
                    "userPrincipalName",
                    "mail",
                    "jobTitle",
                    "department",
                    "accountEnabled",
                    "mobilePhone",
                    "city"
                    ];
                });

                if (user?.AccountEnabled == true)
                {
                    users.Add(new AzureAdUserLookupDto
                    {
                        Id = user.Id ?? string.Empty,
                        DisplayName = user.DisplayName,
                        GivenName = user.GivenName,
                        Surname = user.Surname,
                        UserPrincipalName = user.UserPrincipalName,
                        Mail = user.Mail,
                        JobTitle = user.JobTitle,
                        Department = user.Department,
                        MobilePhone = user.MobilePhone,
                        City = user.City
                    });
                }
            }
            catch
            {
                // Optionally log and continue with the next email.
            }
        }

        return ApiResponse<List<AzureAdUserLookupDto>>.Ok(users);
    }
}