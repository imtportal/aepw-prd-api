using System.Text.Json;
using AEPW.Application.DTOs.Evidence;
using AEPW.Application.Exceptions;
using AEPW.Application.Interfaces.Repositories.Evidence;
using AEPW.Application.Interfaces.Services.Evidence;
using AEPW.Domain.Models.Evidence;
using Microsoft.Extensions.Logging;

namespace AEPW.Application.Services.Evidence;

public sealed class EvidenceReviewService : IEvidenceReviewService
{
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IEvidenceReviewRepository _evidenceReviewRepository;
    private readonly ILogger<EvidenceReviewService> _logger;

    public EvidenceReviewService(
        IEvidenceReviewRepository evidenceReviewRepository,
        ILogger<EvidenceReviewService> logger)
    {
        _evidenceReviewRepository = evidenceReviewRepository;
        _logger = logger;
    }

    public async Task<EvidenceReviewResponseDto> GetReviewDetailsAsync(
        long evidenceId,
        long userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "[EVIDENCE-REVIEW] Fetching review details. EvidenceId={EvidenceId}, UserId={UserId}",
            evidenceId,
            userId);

        if (evidenceId <= 0)
        {
            throw new ValidationException("evidenceId is required and must be greater than zero.");
        }

        if (userId <= 0)
        {
            throw new ValidationException("userId is required and must be greater than zero.");
        }

        var data = await _evidenceReviewRepository.GetEvidenceReviewAsync(
            evidenceId,
            userId,
            cancellationToken);

        if (data is null)
        {
            throw new NotFoundException("Evidence", evidenceId);
        }

        var (validationStatus, validationComment, isWithinReporting) = ParseValidationResult(data.ValidationResult);
        var fieldsExtracted = data.ScalarFields.Count(f => !string.IsNullOrWhiteSpace(f.Value));
        var uploadedAt = FormatTimestamp(data.UploadedUtc);
        var projectName = BuildProjectName(data.ProjectPin, data.ProjectTitle);
        var statusDisplay = data.StatusName ?? data.StatusCode ?? "Pending";

        // Average confidence across all extracted fields (scalars + line items), ignoring nulls.
        var avgConfidenceScore = ComputeAverageConfidence(data);

        var response = new EvidenceReviewResponseDto
        {
            HeaderData = new EvidenceReviewHeaderDto
            {
                SubmissionId = data.EvidenceId.ToString(),
                AgentRunId = null,
                ProjectName = projectName,
                MetricCode = data.MetricCode,
                EvidenceType = data.EvidenceTypeCode ?? data.EvidenceTypeDescription,
                ReportingPeriod = data.ReportingPeriod,
                UploadedAt = uploadedAt,
                EvidenceFile = new EvidenceReviewFileDto
                {
                    FileName = data.FileName,
                    FileType = ResolveFileType(data.FileName),
                    EvidenceUrl = data.FileUriView,
                    DownloadUrl = data.FileUriDownload,
                    FileSize = FormatFileSize(data.FileSizeBytes),
                    UploadedBy = data.UploadedByName ?? data.UploadedByUserId?.ToString() ?? string.Empty,
                    UploadedAt = uploadedAt,
                    BlobContainer = null,
                    SourceLabel = "Source Evidence"
                },
                ExtractionSummary = new EvidenceReviewExtractionSummaryDto
                {
                    Status = statusDisplay,
                    StatusTitle = statusDisplay,
                    StatusMessage = $"{fieldsExtracted} field(s) extracted.",
                    FieldsExtracted = fieldsExtracted,
                    RequiredFields = 0,
                    RulesPassed = 0,
                    Warnings = 0,
                    Failed = 0,
                    AvgConfidenceScore = avgConfidenceScore,
                    AgentQuality = null
                }
            },
            EvidenceDetail = BuildEvidenceDetailPages(data),
            LineItems = new EvidenceReviewLineItemGroupDto
            {
                FieldName = data.LineItems.Count > 0 ? data.LineItems[0].FieldName : null,
                Items = data.LineItems
                    .Select(li => new EvidenceReviewLineItemDto
                    {
                        EvidenceDetailId = li.EvidenceDetailId,
                        EvidenceFieldId = li.EvidenceFieldId,
                        Name = li.Name,
                        Value = li.Value,
                        Confidence = li.ConfidenceScore,
                        PageNumber = li.PageNumber
                    })
                    .ToList()
            },
            Evidenced = new EvidenceReviewEvidencedDto
            {
                FieldName = null,
                EvidencedValue = data.EvidencedValue
            },
            Validation = new EvidenceReviewValidationDto
            {
                ValidationStatus = validationStatus,
                ValidationComment = validationComment,
                IsWithinReporting = isWithinReporting
            },
            RawOutput = new EvidenceReviewRawOutputDto
            {
                Json = ParseRawJson(data.RawJson)
            },
            ValidationData = []
        };

        _logger.LogInformation(
            "[EVIDENCE-REVIEW] Built review details. EvidenceId={EvidenceId}, FieldsExtracted={FieldsExtracted}, LineItems={LineItemCount}, AvgConfidence={AvgConfidence}",
            evidenceId,
            fieldsExtracted,
            data.LineItems.Count,
            avgConfidenceScore);

        return response;
    }



    public async Task<bool> SaveExtractedFieldsAsync(
        long routeEvidenceId,
        SaveEvidenceReviewExtractedFieldsRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (routeEvidenceId <= 0)
        {
            throw new ValidationException(
                "evidenceId is required and must be greater than zero.");
        }

        if (request.EvidenceId <= 0)
        {
            throw new ValidationException(
                "The request evidenceId is required and must be greater than zero.");
        }

        if (request.EvidenceId != routeEvidenceId)
        {
            throw new ValidationException(
                "The route evidenceId must match the request evidenceId.");
        }

        if (request.UpdatedBy <= 0)
        {
            throw new ValidationException(
                "updatedBy is required and must be greater than zero.");
        }

        if (request.Fields is null || request.Fields.Count == 0)
        {
            throw new ValidationException(
                "At least one extracted field must be provided.");
        }

        _logger.LogInformation(
            "[EVIDENCE-REVIEW] Validating extracted-field save. " +
            "EvidenceId={EvidenceId}, FieldCount={FieldCount}, UpdatedBy={UpdatedBy}",
            routeEvidenceId,
            request.Fields.Count,
            request.UpdatedBy);

        var existingReview =
            await _evidenceReviewRepository.GetEvidenceReviewAsync(
                routeEvidenceId,
                request.UpdatedBy,
                cancellationToken);

        if (existingReview is null)
        {
            throw new NotFoundException("Evidence", routeEvidenceId);
        }

        // Index existing scalar fields by EvidenceDetailId.
        var scalarFieldsById = existingReview.ScalarFields
            .Where(field => field.EvidenceDetailId.HasValue)
            .ToDictionary(field => field.EvidenceDetailId!.Value);

        // Index existing line items directly by EvidenceDetailId.
        var lineItemsById = existingReview.LineItems
            .ToDictionary(item => item.EvidenceDetailId);

        var fields = new List<EvidenceReviewExtractedField>(
            request.Fields.Count);

        var requestedDetailIds = new HashSet<long>();

        for (var index = 0; index < request.Fields.Count; index++)
        {
            var field = request.Fields[index];

            if (field is null)
            {
                throw new ValidationException(
                    $"fields[{index}] is required.");
            }

            if (field.EvidenceDetailId <= 0)
            {
                throw new ValidationException(
                    $"fields[{index}].evidenceDetailId must be greater than zero.");
            }

            if (!requestedDetailIds.Add(field.EvidenceDetailId))
            {
                throw new ValidationException(
                    $"EvidenceDetailId '{field.EvidenceDetailId}' was submitted more than once.");
            }

            // Scalar extracted field.
            if (scalarFieldsById.TryGetValue(
                    field.EvidenceDetailId,
                    out var scalarField))
            {
                fields.Add(new EvidenceReviewExtractedField(
                    EvidenceDetailId: scalarField.EvidenceDetailId!.Value,
                    EvidenceFieldId: scalarField.EvidenceFieldId,
                    Value: field.Value,
                    LineItemValue: null,
                    IsLineItem: false,
                    PageNumber: scalarField.PageNumber,
                    ConfidenceScore: scalarField.ConfidenceScore));

                continue;
            }

            // Existing line item.
            if (lineItemsById.TryGetValue(
                    field.EvidenceDetailId,
                    out var lineItem))
            {
                fields.Add(new EvidenceReviewExtractedField(
                    EvidenceDetailId: lineItem.EvidenceDetailId,
                    EvidenceFieldId: lineItem.EvidenceFieldId,
                    Value: lineItem.Name,
                    LineItemValue: field.Value,
                    IsLineItem: true,
                    PageNumber: lineItem.PageNumber,
                    ConfidenceScore: lineItem.ConfidenceScore));

                continue;
            }

            // The submitted detail does not belong to this evidence review.
            throw new ValidationException(
                $"EvidenceDetailId '{field.EvidenceDetailId}' does not identify an extracted field or line item for evidence '{routeEvidenceId}'.");
        }

        await _evidenceReviewRepository.SaveExtractedFieldsAsync(
            routeEvidenceId,
            fields,
            request.UpdatedBy,
            request.EvidencedValue,
            cancellationToken);

        _logger.LogInformation(
            "[EVIDENCE-REVIEW] Saved extracted fields and line items. " +
            "EvidenceId={EvidenceId}, FieldCount={FieldCount}, UpdatedBy={UpdatedBy}",
            routeEvidenceId,
            fields.Count,
            request.UpdatedBy);

        return true;
    }


    /// <summary>
    /// Builds one page group per page NUMBER that exists anywhere for this evidence - not just
    /// pages that happen to have a scalar field. A page with line items but no scalar fields
    /// (e.g. page 7: "data": [], "lineItems": {...}) previously produced NO group at all, since
    /// grouping was keyed only off ScalarFields - the page, and therefore its line items, never
    /// showed up in the UI even though LineItems already carried the correct PageNumber.
    /// Page numbers are the union of: ScalarFields' pages, LineItems' pages, and every page
    /// enumerated in the raw agent JSON (which includes blank pages the agent scanned but found
    /// nothing on) - so even a page with neither scalar fields nor line items now appears, with
    /// an empty Details list. Scalar fields with NO page at all (defined but never extracted on
    /// any page - PageNumber is null from RS2's LEFT JOIN) are kept in one trailing "page: null"
    /// group at the end, exactly as before.
    /// </summary>
    private List<EvidenceReviewPageGroupDto> BuildEvidenceDetailPages(EvidenceReviewData data)
    {
        var scalarFieldsByPage = data.ScalarFields
            .Where(f => f.PageNumber.HasValue)
            .GroupBy(f => f.PageNumber!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var lineItemsByPage = data.LineItems
            .Where(li => li.PageNumber.HasValue)
            .GroupBy(li => li.PageNumber!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var allPageNumbers = new SortedSet<int>(scalarFieldsByPage.Keys);
        allPageNumbers.UnionWith(lineItemsByPage.Keys);
        allPageNumbers.UnionWith(ExtractPageNumbersFromRawJson(data.RawJson));

        var pages = allPageNumbers.Select(pageNumber =>
        {
            scalarFieldsByPage.TryGetValue(pageNumber, out var pageScalarFields);
            lineItemsByPage.TryGetValue(pageNumber, out var pageLineItems);

            var (isUnique, originalPage) = pageScalarFields?.Count > 0
                ? (pageScalarFields[0].IsUnique, pageScalarFields[0].OriginalPage)
                : pageLineItems?.Count > 0
                    ? (pageLineItems[0].IsUnique, pageLineItems[0].OriginalPage)
                    : (true, (int?)null);

            return new EvidenceReviewPageGroupDto
            {
                PageNumber = pageNumber,
                IsUnique = isUnique,
                OriginalPage = originalPage,
                Details = (pageScalarFields ?? [])
                    .Select(f => new EvidenceReviewFieldDto
                    {
                        EvidenceDetailId = f.EvidenceDetailId,
                        EvidenceFieldId = f.EvidenceFieldId,
                        Property = f.EvidenceKey,
                        Label = f.EvidenceKey,
                        Value = f.Value,
                        DataType = "string",
                        Required = ToLowerBoolString(false),
                        ValidationCode = null
                    })
                    .ToList()
            };
        }).ToList();

        // Defined-but-never-extracted-anywhere fields (PageNumber is null) - unchanged from
        // before, kept as one trailing group so the UI can still show them as blank/manual-entry.
        var unassignedFields = data.ScalarFields.Where(f => !f.PageNumber.HasValue).ToList();

        if (unassignedFields.Count > 0)
        {
            pages.Add(new EvidenceReviewPageGroupDto
            {
                PageNumber = null,
                IsUnique = true,
                OriginalPage = null,
                Details = unassignedFields
                    .Select(f => new EvidenceReviewFieldDto
                    {
                        EvidenceDetailId = f.EvidenceDetailId,
                        EvidenceFieldId = f.EvidenceFieldId,
                        Property = f.EvidenceKey,
                        Label = f.EvidenceKey,
                        Value = f.Value,
                        DataType = "string",
                        Required = ToLowerBoolString(false),
                        ValidationCode = null
                    })
                    .ToList()
            });
        }

        return pages;
    }

    /// <summary>
    /// Enumerates every pageNumber in the raw agent JSON's documentResults[].pages[] - including
    /// pages with empty data[]/lineItems, so BuildEvidenceDetailPages can surface them even when
    /// neither EvidenceDetail nor line-item rows exist for them. Never throws - malformed/missing
    /// RawJson just yields no extra pages (the scalar/line-item page numbers still apply).
    /// </summary>
    private IEnumerable<int> ExtractPageNumbersFromRawJson(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(rawJson);
            var root = document.RootElement;

            if (!root.TryGetProperty("documentResults", out var documentResultsElement) ||
                documentResultsElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var pageNumbers = new List<int>();

            foreach (var documentResult in documentResultsElement.EnumerateArray())
            {
                if (!documentResult.TryGetProperty("pages", out var pagesElement) ||
                    pagesElement.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var page in pagesElement.EnumerateArray())
                {
                    if (page.TryGetProperty("pageNumber", out var pageNumberElement) &&
                        pageNumberElement.ValueKind == JsonValueKind.Number &&
                        pageNumberElement.TryGetInt32(out var pageNumber) &&
                        pageNumber >= 1)
                    {
                        pageNumbers.Add(pageNumber);
                    }
                }
            }

            return pageNumbers;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "[EVIDENCE-REVIEW] EvidenceRawExtraction.RawJson was not valid JSON while extracting page numbers.");
            return [];
        }
    }

    /// <summary>
    /// Averages the per-field ConfidenceScore across all extracted fields — scalar fields and
    /// line items — ignoring nulls. Returns null when no field carries a confidence value.
    /// Rounded to 2 decimal places.
    /// </summary>
    private static double? ComputeAverageConfidence(EvidenceReviewData data)
    {
        var confidences = data.ScalarFields
            .Where(f => f.ConfidenceScore.HasValue)
            .Select(f => f.ConfidenceScore!.Value)
            .Concat(data.LineItems
                .Where(li => li.ConfidenceScore.HasValue)
                .Select(li => li.ConfidenceScore!.Value))
            .ToList();

        return confidences.Count > 0
            ? Math.Round(confidences.Average(), 2)
            : (double?)null;
    }

    /// <summary>
    /// Parses Evidence.ValidationResult (written by /process as {"ValidationStatus",
    /// "ValidationComment","IsWithinReporting"}). IsWithinReporting is only present on rows
    /// reprocessed after that key was added - older rows simply yield null for it.
    /// </summary>
    private (string? Status, string? Comment, string? IsWithinReporting) ParseValidationResult(string? validationResultJson)
    {
        if (string.IsNullOrWhiteSpace(validationResultJson))
        {
            return (null, null, null);
        }

        try
        {
            using var document = JsonDocument.Parse(validationResultJson);
            var root = document.RootElement;

            var status = root.TryGetProperty("ValidationStatus", out var statusElement) && statusElement.ValueKind == JsonValueKind.String
                ? statusElement.GetString()
                : null;

            var comment = root.TryGetProperty("ValidationComment", out var commentElement) && commentElement.ValueKind == JsonValueKind.String
                ? commentElement.GetString()
                : null;

            var isWithinReporting = GetStringProperty(root, "IsWithinReporting", "isWithinReporting");

            return (status, comment, isWithinReporting);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "[EVIDENCE-REVIEW] Evidence.ValidationResult was not valid JSON; returning null.");
            return (null, null, null);
        }
    }

    /// <summary>Tries each candidate property name in order (case-sensitive each), returning the first string match.</summary>
    private static string? GetStringProperty(JsonElement root, params string[] candidateNames)
    {
        foreach (var name in candidateNames)
        {
            if (root.TryGetProperty(name, out var element) && element.ValueKind == JsonValueKind.String)
            {
                return element.GetString();
            }
        }

        return null;
    }

    private object? ParseRawJson(string? rawJson)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<JsonElement>(rawJson, CaseInsensitiveJson);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "[EVIDENCE-REVIEW] EvidenceRawExtraction.RawJson was not valid JSON; returning null.");
            return null;
        }
    }

    private static string BuildProjectName(string? pin, string? title)
    {
        if (string.IsNullOrWhiteSpace(pin))
        {
            return title ?? string.Empty;
        }

        return string.IsNullOrWhiteSpace(title) ? pin : $"{pin} - {title}";
    }

    /// <summary>
    /// Converts a bool to a lowercase JSON-style string ("true"/"false"), never C#'s
    /// default bool.ToString() casing ("True"/"False").
    /// </summary>
    private static string ToLowerBoolString(bool value) => value ? "true" : "false";

    private static string FormatTimestamp(DateTime value) =>
        value == DateTime.MinValue ? string.Empty : value.ToString("dd-MMM-yyyy HH:mm");

    private static string FormatFileSize(long? bytes)
    {
        if (bytes is null or <= 0)
        {
            return string.Empty;
        }

        const double kb = 1024d;
        const double mb = kb * 1024d;

        return bytes.Value switch
        {
            >= (long)mb => $"{bytes.Value / mb:0.#} MB",
            >= (long)kb => $"{bytes.Value / kb:0.#} KB",
            _ => $"{bytes.Value} B"
        };
    }

    private static string ResolveFileType(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        var extension = Path.GetExtension(fileName).TrimStart('.').ToUpperInvariant();

        return extension switch
        {
            "JPG" or "JPEG" => "JPEG",
            "" => string.Empty,
            _ => extension
        };
    }
}