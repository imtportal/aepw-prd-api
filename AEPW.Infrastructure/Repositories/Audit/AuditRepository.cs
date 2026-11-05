using System.Data;
using AEPW.Application.DTOs.Audit;
using AEPW.Application.Interfaces.Repositories.Audit;
using AEPW.Domain.Constants;
using AEPW.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace AEPW.Infrastructure.Repositories.Audit;

public sealed class AuditRepository : IAuditRepository
{
    private readonly ISqlConnectionFactory _sqlConnectionFactory;

    public AuditRepository(ISqlConnectionFactory sqlConnectionFactory)
    {
        _sqlConnectionFactory = sqlConnectionFactory;
    }

    public async Task<List<AuditTimelineItemDto>> GetAuditTimelineAsync(
        GetAuditTimelineRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var result = new List<AuditTimelineItemDto>();

        // Guard: If module requires a project and none was provided, return empty list without hitting database
        bool isProjectRequired = request.Module == AuditModuleType.ReviewLifecycle
                              || request.Module == AuditModuleType.ReviewApproval
                              || request.Module == AuditModuleType.Restatement;

        if (isProjectRequired && (!request.ProjectId.HasValue || request.ProjectId.Value <= 0))
        {
            return new List<AuditTimelineItemDto>();
        }

        switch (request.Module)
        {
            case AuditModuleType.UserManagement:
                var userTask = ExecuteAuditSpAsync(
                    StoredProcedures.Audit.GetAppUserAudit,
                    request,
                    "User Management",
                    "AppUser",
                    cancellationToken: cancellationToken);

                var roleTask = ExecuteAuditSpAsync(
                    StoredProcedures.Audit.GetAppUserSystemRoleAudit,
                    request,
                    "User Management",
                    "AppUserSystemRole",
                    cancellationToken: cancellationToken);

                var userRoleResults = await Task.WhenAll(userTask, roleTask);
                result.AddRange(userRoleResults[0]);
                result.AddRange(userRoleResults[1]);
                break;

            case AuditModuleType.MetricMaster:
                var metricLogs = await ExecuteAuditSpAsync(
                    StoredProcedures.Audit.GetMetricAudit,
                    request,
                    "Metric Master",
                    "Metric",
                    sqlPaginated: true,       // Strategy A: SQL handles OFFSET/FETCH
                    cancellationToken: cancellationToken);
                result.AddRange(metricLogs);
                break;

            case AuditModuleType.ImpactMaster:
                var impactTask = ExecuteAuditSpAsync(
                    StoredProcedures.Audit.GetImpactMasterAudit,
                    request,
                    "Project Master",
                    "ImpactMaster",
                    includeProgramId: true,
                    includeProjectId: true,
                    cancellationToken: cancellationToken);

                var impactMetricTask = ExecuteAuditSpAsync(
                    StoredProcedures.Audit.GetImpactMasterMetricAudit,
                    request,
                    "Project Master",
                    "ImpactMasterMetric",
                    includeProgramId: true,
                    includeProjectId: true,
                    cancellationToken: cancellationToken);

                var impactResults = await Task.WhenAll(impactTask, impactMetricTask);
                result.AddRange(impactResults[0]);
                result.AddRange(impactResults[1]);
                break;

            case AuditModuleType.ReviewLifecycle:
                var reviewTask = ExecuteAuditSpAsync(
                    StoredProcedures.Audit.GetReviewAudit,
                    request,
                    "Review Lifecycle",
                    "Review",
                    includeProgramId: true,
                    includeProjectId: true,
                    cancellationToken: cancellationToken);

                var reviewMetricTask = ExecuteAuditSpAsync(
                    StoredProcedures.Audit.GetReviewMetricAudit,
                    request,
                    "Review Lifecycle",
                    "ReviewMetric",
                    includeProgramId: true,
                    includeProjectId: true,
                    includeMetricCode: true,
                    cancellationToken: cancellationToken);

                var reviewResults = await Task.WhenAll(reviewTask, reviewMetricTask);
                result.AddRange(reviewResults[0]);
                result.AddRange(reviewResults[1]);
                break;

            case AuditModuleType.ReviewApproval:
                var approvalLogs = await ExecuteAuditSpAsync(
                    StoredProcedures.Audit.GetReviewApprovalAudit,
                    request,
                    "Review Approval",
                    "ReviewApproval",
                    includeProgramId: true,
                    includeProjectId: true,
                    includeMetricCode: true,
                    sqlPaginated: true,       // Strategy A: SQL handles OFFSET/FETCH
                    cancellationToken: cancellationToken);
                result.AddRange(approvalLogs);
                break;

            case AuditModuleType.Restatement:
                var restatementLogs = await ExecuteAuditSpAsync(
                    StoredProcedures.Audit.GetRestatementAudit,
                    request,
                    "Restatement",
                    "ReviewMetric",
                    includeProgramId: true,
                    includeProjectId: true,
                    includeMetricCode: true,
                    sqlPaginated: true,       // Strategy A: SQL handles OFFSET/FETCH
                    cancellationToken: cancellationToken);
                result.AddRange(restatementLogs);
                break;
        }

        // Sort all results by ChangedOn descending (required for dual-SP merge correctness)
        var orderedResult = result.OrderByDescending(x => x.ChangedOn).AsEnumerable();

        // Strategy A modules (MetricMaster, ReviewApproval, Restatement): SQL already did OFFSET/FETCH.
        // Applying Skip/Take again on those 20 rows would return 0 items on Page 2+.
        // Strategy B modules (dual-SP): C# merges two bounded lists and final-slices here.
        bool isSqlPaginated = request.Module == AuditModuleType.MetricMaster
                           || request.Module == AuditModuleType.ReviewApproval
                           || request.Module == AuditModuleType.Restatement;

        if (!isSqlPaginated && request.PageSize > 0 && request.PageNumber > 0)
        {
            var skip = (request.PageNumber - 1) * request.PageSize;
            return orderedResult.Skip(skip).Take(request.PageSize).ToList();
        }

        return orderedResult.ToList();
    }

    private async Task<List<AuditTimelineItemDto>> ExecuteAuditSpAsync(
        string spName,
        GetAuditTimelineRequestDto request,
        string moduleName,
        string entityName,
        bool includeProgramId = false,
        bool includeProjectId = false,
        bool includeMetricCode = false,
        bool sqlPaginated = false,        // true = SQL already applied OFFSET/FETCH (Strategy A)
        CancellationToken cancellationToken = default)
    {
        var logs = new List<AuditTimelineItemDto>();

        await using var connection = await _sqlConnectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var command = new SqlCommand(spName, connection);
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.AddWithValue("@FromDate", (object?)request.FromDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@ToDate", (object?)request.ToDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@UserId", request.UserId.HasValue ? (object)request.UserId.Value : DBNull.Value);

        if (includeProjectId)
        {
            command.Parameters.AddWithValue("@ImpactMasterId", request.ProjectId.HasValue ? (object)request.ProjectId.Value : DBNull.Value);
        }

        if (includeProgramId)
        {
            command.Parameters.AddWithValue("@ProgramId", request.ProgramId.HasValue ? (object)request.ProgramId.Value : DBNull.Value);
        }

        // Pass SearchText to SP — SQL WHERE clause now handles the filtering
        command.Parameters.AddWithValue("@SearchText",
            !string.IsNullOrWhiteSpace(request.SearchText)
                ? (object)request.SearchText.Trim()
                : DBNull.Value);

        // Pass EventAction to SP — SQL WHERE clause filters by action ('created' / 'modified')
        command.Parameters.AddWithValue("@EventAction",
            !string.IsNullOrWhiteSpace(request.EventAction)
                ? (object)request.EventAction.Trim()
                : DBNull.Value);

        if (includeMetricCode)
        {
            command.Parameters.AddWithValue("@MetricCode",
                !string.IsNullOrWhiteSpace(request.MetricCode)
                    ? (object)request.MetricCode.Trim()
                    : DBNull.Value);
        }

        // Pass pagination params — SQL uses these to cap or directly OFFSET results
        command.Parameters.AddWithValue("@PageNumber", request.PageNumber > 0 ? request.PageNumber : 1);
        command.Parameters.AddWithValue("@PageSize",   request.PageSize  > 0 ? request.PageSize  : 20);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        // Build ordinal lookup dictionary once per result set to eliminate IndexOutOfRangeException first-chance exceptions
        var ordinals = new Dictionary<string, int>(reader.FieldCount, StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            ordinals[reader.GetName(i)] = i;
        }

        string? GetColumnValue(string columnName)
        {
            if (ordinals.TryGetValue(columnName, out int ord) && !reader.IsDBNull(ord))
            {
                return reader[ord]?.ToString();
            }
            return null;
        }

        while (await reader.ReadAsync(cancellationToken))
        {
            string? impactMasterId = GetColumnValue("ImpactMasterId");
            string? title = GetColumnValue("Title");
            string? code = GetColumnValue("Code");
            string? name = GetColumnValue("Name");
            string? userName = GetColumnValue("UserName");
            string? pin = GetColumnValue("Pin");

            string? recordTitle = null;

            if (moduleName == "Project Master")
            {
                if (!string.IsNullOrEmpty(impactMasterId) && !string.IsNullOrEmpty(title))
                {
                    recordTitle = $"IM-{impactMasterId} - {title}";
                }
                else if (!string.IsNullOrEmpty(impactMasterId))
                {
                    recordTitle = $"IM-{impactMasterId}";
                }
                else
                {
                    recordTitle = title ?? pin;
                }
            }
            else if (moduleName == "Metric Master")
            {
                if (!string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(name))
                {
                    recordTitle = $"{code} - {name}";
                }
                else
                {
                    recordTitle = code ?? name;
                }
            }
            else if (moduleName == "Review Lifecycle" || moduleName == "Review Approval" || moduleName == "Restatement")
            {
                string? stageName = GetColumnValue("StageName");
                string? reviewId = GetColumnValue("ReviewId");
                string? periodCode = GetColumnValue("PeriodCode") ?? GetColumnValue("ReviewPeriod");
                string? metricName = GetColumnValue("MetricName");
                string? metricCode = GetColumnValue("MetricCode");

                // Period tag in brackets, e.g. " [2024-Q1]"
                string periodTag = !string.IsNullOrEmpty(periodCode) ? $" [{periodCode}]" : "";

                var details = new List<string>();

                // Format Review # from reviewId or from stageName if it starts with "Review #"
                string? revLabel = null;
                if (!string.IsNullOrEmpty(reviewId))
                {
                    revLabel = reviewId.StartsWith("Review #", StringComparison.OrdinalIgnoreCase)
                        ? reviewId
                        : $"Review #{reviewId}";
                }
                else if (!string.IsNullOrEmpty(stageName) && stageName.StartsWith("Review #", StringComparison.OrdinalIgnoreCase))
                {
                    revLabel = stageName;
                }

                // If in Review Lifecycle (Review Metric module), always include Review #
                if (moduleName == "Review Lifecycle" && !string.IsNullOrEmpty(revLabel))
                {
                    details.Add(revLabel);
                }
                else if (string.IsNullOrEmpty(periodCode) && !string.IsNullOrEmpty(revLabel))
                {
                    details.Add(revLabel);
                }

                // If stageName is an actual approval stage name (e.g. "ICP", "L1 Review", etc.), add it
                if (!string.IsNullOrEmpty(stageName) && !stageName.StartsWith("Review #", StringComparison.OrdinalIgnoreCase))
                {
                    details.Add(stageName);
                }

                if (!string.IsNullOrEmpty(metricCode)) details.Add(metricCode);
                else if (!string.IsNullOrEmpty(metricName)) details.Add(metricName);

                string detailSuffix = details.Count > 0 ? $" ({string.Join(" - ", details)})" : "";

                if (!string.IsNullOrEmpty(impactMasterId))
                {
                    recordTitle = $"IM-{impactMasterId}{periodTag}{detailSuffix}";
                }
                else
                {
                    recordTitle = title ?? userName ?? "Approval";
                }
            }
            else
            {
                recordTitle = userName ?? pin ?? title ?? code;
            }

            var item = new AuditTimelineItemDto
            {
                ModuleName = moduleName,
                EntityName = GetColumnValue("EntityName") ?? entityName,
                FieldName = GetColumnValue("FieldName"),
                OldValue = GetColumnValue("OldValue"),
                NewValue = GetColumnValue("NewValue"),
                ChangedBy = GetColumnValue("ChangedBy") ?? "System",
                ChangedOn = ordinals.TryGetValue("ChangedOn", out int changedOnOrd) && !reader.IsDBNull(changedOnOrd)
                    ? reader.GetDateTime(changedOnOrd)
                    : DateTime.UtcNow,
                RecordTitle = recordTitle,
                EventAction = GetColumnValue("EventAction") ?? "modified",
                Comment = GetColumnValue("Comment")
            };

            logs.Add(item);
        }

        return logs;
    }
}
