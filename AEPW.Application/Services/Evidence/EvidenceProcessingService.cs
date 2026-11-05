using System.ClientModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using AEPW.Application.DTOs.Evidence;
using AEPW.Application.Exceptions;
using AEPW.Application.Interfaces.Repositories.Evidence;
using AEPW.Application.Interfaces.Services.Evidence;
using AEPW.Application.Interfaces.Services.ExcelAgent;
using AEPW.Application.Interfaces.Services.ExchangeRate;
using AEPW.Application.Interfaces.Services.FilePreprocessing;
using AEPW.Application.Interfaces.Services.Foundry;
using AEPW.Domain.Constants;
using AEPW.Domain.Models.Evidence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AEPW.Application.Services.Evidence;

public sealed class EvidenceProcessingService : IEvidenceProcessingService
{
    /// <summary>
    /// xlsx/xlsm route to IExcelAgentService (a code-interpreter agent that reads the workbook
    /// directly) - they never reach IFilePreprocessor or the main IFoundryAgentService.
    /// </summary>
    private static readonly HashSet<string> ExcelAgentExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xlsx", ".xlsm"
    };

    private readonly IEvidenceProcessingRepository _evidenceProcessingRepository;
    private readonly IFoundryAgentService _foundryAgentService;
    private readonly IExcelAgentService _excelAgentService;
    private readonly IFilePreprocessor _filePreprocessor;
    private readonly IExchangeRateService _exchangeRateService;
    private readonly EvidenceProcessingSettings _settings;
    private readonly ILogger<EvidenceProcessingService> _logger;

    public EvidenceProcessingService(
        IEvidenceProcessingRepository evidenceProcessingRepository,
        IFoundryAgentService foundryAgentService,
        IExcelAgentService excelAgentService,
        IFilePreprocessor filePreprocessor,
        IExchangeRateService exchangeRateService,
        IOptions<EvidenceProcessingSettings> settings,
        ILogger<EvidenceProcessingService> logger)
    {
        _evidenceProcessingRepository = evidenceProcessingRepository;
        _foundryAgentService = foundryAgentService;
        _excelAgentService = excelAgentService;
        _filePreprocessor = filePreprocessor;
        _exchangeRateService = exchangeRateService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<ProcessEvidenceResponseDto> ProcessAsync(
        ProcessEvidenceRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        // ── STEP 1: validate ────────────────────────────────────────────────
        _logger.LogInformation(
            "[EVIDENCE-PROCESS][STEP 1] Received request. EvidenceId={EvidenceId}, EvidenceType={EvidenceType}, TlmMetricCode={TlmMetricCode}, ReportingYear={ReportingYear}, ReportingQuarter={ReportingQuarter}, L1Metric={L1Metric}, FileSize={Size}",
            request.EvidenceId,
            request.EvidenceType,
            request.TlmMetricCode,
            request.ReportingYear,
            request.ReportingQuarter,
            request.L1MetricCode,
            request.File?.Length ?? 0);

        ValidateRequest(request);

        try
        {
            // ── STEP 1b: mark as Processing immediately ──────────────────────
            await _evidenceProcessingRepository.SetStatusAsync(
                request.EvidenceId,
                EvidenceStatusIds.Processing,
                _settings.EvidenceServiceUserId,
                cancellationToken);

            _logger.LogInformation(
                "[EVIDENCE-PROCESS][STEP 1] StatusId set to {StatusId} (Processing).",
                EvidenceStatusIds.Processing);

            // ── STEP 2: resolve EvidenceTypeId (needed for STEP 5's field list) ──────
            // EvidenceType.Code is the sole lookup key - NOT metric-scoped.
            _logger.LogInformation("[EVIDENCE-PROCESS][STEP 2] Resolving EvidenceType...");

            var evidenceType = await _evidenceProcessingRepository.GetEvidenceTypeByCodeAsync(
                request.EvidenceType,
                cancellationToken);

            if (evidenceType is null)
            {
                throw new NotFoundException($"Evidence type '{request.EvidenceType}' not found.");
            }

            _logger.LogInformation(
                "[EVIDENCE-PROCESS][STEP 2] Resolved. EvidenceTypeId={EvidenceTypeId}, IsExchangeRateRequired={IsExchangeRateRequired}",
                evidenceType.EvidenceTypeId,
                evidenceType.IsExchangeRateRequired);

            // ── STEP 3: route by file type, then call the matching agent ────────────
            // .xlsx/.xlsm -> the Excel agent (code interpreter reads the workbook directly;
            // never touches FilePreprocessor or the main Foundry agent). Everything else ->
            // classify via FilePreprocessor (convert/passthrough/reject), then the main agent.
            await using var fileStream = request.File.OpenReadStream();

            var extension = Path.GetExtension(request.File.FileName);
            FoundryResult foundryResult;

            if (ExcelAgentExtensions.Contains(extension))
            {
                _logger.LogInformation(
                    "[EVIDENCE-PROCESS][STEP 3] '{Extension}' routes to the Excel agent...",
                    extension);

                try
                {
                    foundryResult = await _excelAgentService.ProcessAsync(
                        fileStream,
                        request.File.FileName,
                        request.TlmMetricCode,
                        request.L1MetricCode,
                        request.EvidenceType,
                        request.ReportingYear,
                        request.ReportingQuarter,
                        cancellationToken);
                }
                catch (Exception ex) when (ClassifyAgentCallFailure(ex, cancellationToken) is { } transientReason)
                {
                    throw new TransientProcessingException($"Excel agent call failed transiently: {transientReason}", ex);
                }

                _logger.LogInformation(
                    "[EVIDENCE-PROCESS][STEP 3] Excel agent returned {Length} characters of output.",
                    foundryResult.OutputText.Length);
            }
            else
            {
                var preprocessed = _filePreprocessor.Process(fileStream, request.File.FileName);

                if (preprocessed.Outcome == PreprocessOutcome.Unsupported)
                {
                    _logger.LogWarning(
                        "[EVIDENCE-PROCESS][STEP 3] Rejecting EvidenceId={EvidenceId}: {Reason}",
                        request.EvidenceId,
                        preprocessed.RejectReason);

                    throw new ValidationException(preprocessed.RejectReason ?? "Unsupported file type.");
                }

                _logger.LogInformation(
                    "[EVIDENCE-PROCESS][STEP 3] File classified as {Outcome}. Calling Foundry agent...",
                    preprocessed.Outcome);

                try
                {
                    foundryResult = await _foundryAgentService.ProcessAsync(
                        fileStream,
                        request.File.FileName,
                        request.File.ContentType,
                        request.TlmMetricCode,
                        request.EvidenceType,
                        request.ReportingYear,
                        request.ReportingQuarter,
                        preprocessed,
                        cancellationToken);
                }
                catch (Exception ex) when (ClassifyAgentCallFailure(ex, cancellationToken) is { } transientReason)
                {
                    throw new TransientProcessingException($"Foundry agent call failed transiently: {transientReason}", ex);
                }

                _logger.LogInformation(
                    "[EVIDENCE-PROCESS][STEP 3] Foundry agent returned {Length} characters of output.",
                    foundryResult.OutputText.Length);
            }

            _logger.LogDebug(
                "[EVIDENCE-PROCESS][STEP 3] Foundry output: {OutputText}",
                foundryResult.OutputText);

            // ── STEP 4: parse + flatten the agent's nested JSON output ───────────
            // Refusal/non-JSON check BEFORE attempting to parse: an agent refusal or stray prose
            // is never going to become valid JSON on retry, so it's classified PERMANENT here
            // without ever reaching JsonDocument.Parse.
            var strippedForRefusalCheck = StripJsonFences(foundryResult.OutputText).TrimStart();

            if (!strippedForRefusalCheck.StartsWith('{'))
            {
                _logger.LogError(
                    "[EVIDENCE-PROCESS][STEP 4][PERMANENT] Agent returned non-JSON output (possible refusal) for EvidenceId={EvidenceId}. First 500 chars: {Preview}",
                    request.EvidenceId,
                    Truncate(foundryResult.OutputText, 500));

                throw new PermanentProcessingException("Agent returned a non-JSON response (possible refusal).");
            }

            ParseResult parseResult;

            try
            {
                parseResult = ParseAndFlatten(foundryResult.OutputText);
            }
            catch (JsonException ex)
            {
                _logger.LogError(
                    ex,
                    "[EVIDENCE-PROCESS][STEP 4][PERMANENT] Agent returned malformed JSON for EvidenceId={EvidenceId}. First 500 chars: {Preview}",
                    request.EvidenceId,
                    Truncate(foundryResult.OutputText, 500));

                throw new PermanentProcessingException("Agent returned malformed JSON that could not be parsed.", ex);
            }

            var rows = parseResult.Rows;
            var validationStatus = parseResult.ValidationStatus;
            var evidencedValue = parseResult.EvidencedValue;

            _logger.LogInformation(
                "[EVIDENCE-PROCESS][STEP 4] Parsed {Count} rows across all documentResults/pages. ValidationStatus={ValidationStatus}. FieldNames=[{FieldNames}]",
                rows.Count,
                validationStatus ?? "(none)",
                string.Join(", ", rows.Select(r => r.FieldName).Distinct()));

            // ── STEP 4b: deterministically confirm the LLM's per-page duplicate flags ──
            var pageUniqueness = ConfirmPageUniqueness(rows, parseResult.PageMeta);

            _logger.LogInformation(
                "[EVIDENCE-PROCESS][STEP 4b] Confirmed page uniqueness for {PageCount} page(s). Duplicates={DuplicateCount}",
                pageUniqueness.Count,
                pageUniqueness.Count(kv => !kv.Value.IsUnique));

            // ── STEP 5: resolve MetricEvidenceTypeId, then load allowed keys ─────
            // CORRECTED 2026-10-21: the same EvidenceType is attached to multiple metrics
            // through the MetricEvidenceType junction, and each pairing has its own seeded
            // EvidenceField set - EvidenceTypeId alone is NOT enough (it previously resolved
            // the wrong metric's fields, e.g. an unrelated Invoice pairing's [From, To,
            // Quantity, invoices_line-item] instead of this evidence's own fields).
            // EvidenceTypeId (STEP 2) is still correct for the type/model/FX-flag lookup -
            // only the field-definition resolution switches to MetricEvidenceTypeId.
            // Passes evidenceType.EvidenceTypeId explicitly rather than letting the SP read
            // Evidence.EvidenceTypeId - that column is not reliably populated at process-time
            // (confirmed 2026-10-21: EvidenceId 312 had no matching Evidence.EvidenceTypeId row).
            var metricEvidenceTypeId = await _evidenceProcessingRepository.GetMetricEvidenceTypeIdByEvidenceIdAsync(
                request.EvidenceId,
                evidenceType.EvidenceTypeId,
                cancellationToken);

            if (metricEvidenceTypeId is null)
            {
                throw new NotFoundException(
                    $"Could not resolve MetricEvidenceTypeId for EvidenceId {request.EvidenceId} (EvidenceTypeId={evidenceType.EvidenceTypeId}).");
            }

            _logger.LogInformation(
                "[EVIDENCE-PROCESS][STEP 5] Resolved MetricEvidenceTypeId={MetricEvidenceTypeId} for EvidenceId={EvidenceId}.",
                metricEvidenceTypeId.Value,
                request.EvidenceId);

            var fields = await _evidenceProcessingRepository.GetFieldsByTypeIdAsync(
                metricEvidenceTypeId.Value,
                cancellationToken);

            _logger.LogInformation(
                "[EVIDENCE-PROCESS][STEP 5] Loaded {Count} field definitions. EvidenceKeys=[{EvidenceKeys}]",
                fields.Count,
                string.Join(", ", fields.Select(f => f.EvidenceKey)));

            // ── STEP 6: filter rows down to the defined EvidenceKeys ─────────────
            var details = FilterToDefinedFields(rows, fields, pageUniqueness).ToList();

            _logger.LogInformation(
                "[EVIDENCE-PROCESS][STEP 6] Filtered to {Count} matching EvidenceDetail rows.",
                details.Count);

            if (details.Count == 0 && rows.Count > 0)
            {
                _logger.LogWarning(
                    "[EVIDENCE-PROCESS][STEP 6] 0 rows matched out of {RowCount} extracted - none of the agent's FieldNames normalized to one of STEP 5's EvidenceKeys. UnmatchedFieldNames=[{UnmatchedFieldNames}]",
                    rows.Count,
                    string.Join(", ", rows.Select(r => r.FieldName).Distinct()));
            }

            // ── STEP 6b: conditional, deterministic USD conversion (best-effort) ─
            // Gated explicitly on EvidenceType.IsExchangeRateRequired (STEP 2) - NOT on whether
            // the agent happened to emit currency fields. Flag off -> complete no-op, no FX call,
            // no FX fields touched. Flag on but data missing/NA/unparsable is handled INSIDE
            // ApplyFxConversionAsync (per-page guards) - it never throws; any skip/failure there
            // just leaves ExchangeRate/Rate Date/USD Converted Amount empty for manual entry.
            //
            // rawJsonToSave: default is the unmodified agent output; replaced with the FX-mutated
            // version (same numbers as EvidenceDetail) when IsExchangeRateRequired=true and at
            // least one page converted successfully.
            var rawJsonToSave = foundryResult.OutputText;

            if (evidenceType.IsExchangeRateRequired)
            {
                _logger.LogInformation(
                    "[EVIDENCE-PROCESS][STEP 6b][FX] IsExchangeRateRequired=true for this EvidenceType; attempting FX conversion.");

                try
                {
                    var (fxEvidencedValue, fxModifiedJson) = await ApplyFxConversionAsync(
                        details, fields, pageUniqueness, foundryResult.OutputText, cancellationToken);

                    rawJsonToSave = fxModifiedJson;

                    if (fxEvidencedValue is not null)
                    {
                        _logger.LogInformation(
                            "[EVIDENCE-PROCESS][STEP 6b][FX] Overriding EvidencedValue with aggregated USD total - OldValue={OldValue}, NewValue={NewValue}.",
                            evidencedValue ?? "(none)",
                            fxEvidencedValue);

                        evidencedValue = fxEvidencedValue;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "[EVIDENCE-PROCESS][STEP 6b][FX] FX conversion step failed unexpectedly; continuing without FX values.");
                }
            }
            else
            {
                _logger.LogInformation(
                    "[EVIDENCE-PROCESS][STEP 6b][FX] IsExchangeRateRequired=false for this EvidenceType; skipping FX conversion entirely.");
            }

            // ── STEP 7: persist details ──────────────────────────────────────────
            await _evidenceProcessingRepository.SaveDetailsAsync(
                request.EvidenceId,
                details,
                _settings.EvidenceServiceUserId,
                cancellationToken);

            _logger.LogInformation("[EVIDENCE-PROCESS][STEP 7] Saved details to DB.");

            // ── STEP 8: store raw JSON ───────────────────────────────────────────
            // For FX evidence types, rawJsonToSave is the mutated JSON (FX fields + aggregated
            // evidenced value filled in) - same numbers already written to EvidenceDetail and
            // Evidence.EvidencedValue. For non-FX types it is the pristine agent output.
            await _evidenceProcessingRepository.SaveRawExtractionAsync(
                request.EvidenceId,
                rawJsonToSave,
                foundryResult.AgentIdentifier ?? string.Empty,
                cancellationToken);

            _logger.LogInformation("[EVIDENCE-PROCESS][STEP 8] Saved raw extraction JSON.");

            // ── STEP 9: set review flag + status (single SP call) ───────────────
            var isReviewRequired = DetermineIsReviewRequired(validationStatus);

            var statusCode = isReviewRequired
                ? EvidenceStatuses.NeedsReview
                : EvidenceStatuses.Validated;

            var isWithinReporting = MapIsWithinReporting(parseResult.IsWithinReportingRaw);

            // Out-of-reporting override: IsWithinReporting=0 forces NeedsReview regardless
            // of what the agent's validationStatus said - a reviewer must inspect it.
            if (!isWithinReporting)
            {
                isReviewRequired = true;
                statusCode       = EvidenceStatuses.NeedsReview;
            }

            // Confidence threshold override: average confidence < 80% forces NeedsReview.
            // Only applies when at least one detail row carries a confidence score; evidence
            // where the agent emitted no confidence values at all is not penalised.
            var scoredDetails = details.Where(d => d.ConfidenceScore.HasValue).ToList();
            double? avgConfidence = scoredDetails.Count > 0
                ? scoredDetails.Average(d => d.ConfidenceScore!.Value)
                : null;

            if (avgConfidence.HasValue && avgConfidence.Value < ConfidenceReviewThreshold)
            {
                isReviewRequired = true;
                statusCode       = EvidenceStatuses.NeedsReview;
            }

            await _evidenceProcessingRepository.UpdateProcessingResultAsync(
                request.EvidenceId,
                isReviewRequired,
                statusCode,
                evidencedValue,
                parseResult.ValidationResult,
                isWithinReporting,
                _settings.EvidenceServiceUserId,
                cancellationToken);

            _logger.LogInformation(
                "[EVIDENCE-PROCESS][STEP 9] StatusCode set to {StatusCode} (IsReviewRequired={IsReviewRequired}, IsWithinReporting={IsWithinReporting}, raw='{Raw}', ValidationStatus={ValidationStatus}, AvgConfidence={AvgConfidence}).",
                statusCode,
                isReviewRequired,
                isWithinReporting,
                parseResult.IsWithinReportingRaw ?? "(none)",
                validationStatus ?? "(none)",
                avgConfidence.HasValue ? avgConfidence.Value.ToString("F4") : "(none)");

            // ── STEP 10: recompute top-metric adjusted values (best-effort) ──────
            // The evidence result is already saved (STEP 9) - a failure here must never
            // fail the request or flip the evidence back to Failed.
            try
            {
                var reviewId = await _evidenceProcessingRepository.GetReviewIdByEvidenceIdAsync(
                    request.EvidenceId,
                    cancellationToken);

                if (reviewId.HasValue)
                {
                    _logger.LogInformation(
                        "[EVIDENCE-PROCESS][STEP 10] Updating raw adjusted values. ReviewId={ReviewId}",
                        reviewId.Value);

                    var (data, message) = await _evidenceProcessingRepository.UpdateRawAdjustedValuesForTopMetricsAsync(
                        reviewId.Value,
                        cancellationToken);

                    _logger.LogInformation(
                        "[EVIDENCE-PROCESS][STEP 10] Raw adjusted values updated. ReviewId={ReviewId}, Data={Data}, Message={Message}",
                        reviewId.Value,
                        data ?? "(none)",
                        message ?? "(none)");
                }
                else
                {
                    _logger.LogWarning(
                        "[EVIDENCE-PROCESS][STEP 10] Could not resolve ReviewId for EvidenceId={EvidenceId}; skipping adjusted-values update.",
                        request.EvidenceId);
                }
            }
            catch (Exception ex)
            {
                // Best-effort: never fail evidence processing because the adjusted-values recompute failed.
                _logger.LogError(
                    ex,
                    "[EVIDENCE-PROCESS][STEP 10] uspUpdateRawAdjustedValuesForTopMetrics failed for EvidenceId={EvidenceId}; evidence is saved, continuing.",
                    request.EvidenceId);
            }

            stopwatch.Stop();

            _logger.LogInformation(
                "[EVIDENCE-PROCESS][DONE] EvidenceId={EvidenceId} in {Ms}ms.",
                request.EvidenceId,
                stopwatch.ElapsedMilliseconds);

            return new ProcessEvidenceResponseDto
            {
                EvidenceId = request.EvidenceId,
                FieldsExtracted = details.Count,
                IsReviewRequired = isReviewRequired
            };
        }
        catch (TransientProcessingException ex)
        {
            // TRANSIENT: do NOT mark Failed. Evidence is left at Processing (set in STEP 1b) so a
            // Worker-driven retry of this same EvidenceId can still complete successfully - see
            // the response-contract comment on EvidenceProcessingController.ProcessEvidence.
            _logger.LogWarning(
                ex,
                "[EVIDENCE-PROCESS][TRANSIENT] EvidenceId={EvidenceId} - retryable failure, leaving status as Processing.",
                request.EvidenceId);

            throw;
        }
        catch (Exception ex)
        {
            // PERMANENT (or unclassified/unexpected - treated the same, since leaving evidence
            // stuck in Processing forever is worse than a Failed row a human can re-trigger).
            _logger.LogError(
                ex,
                "[EVIDENCE-PROCESS][FAILED] EvidenceId={EvidenceId} - marking StatusId={StatusId} (Failed). Reason: {Reason}",
                request.EvidenceId,
                EvidenceStatusIds.Failed,
                ex is PermanentProcessingException ? ex.Message : "Unexpected error");

            try
            {
                // Fresh, non-request token: the request's own cancellationToken may already be
                // cancelled (e.g. the agent call hit NetworkTimeoutSeconds and cancelled it), which
                // would otherwise make this status write fail too and leave the evidence stuck in
                // Processing forever. 30s is ample for a single status-update SP call.
                using var statusCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                await _evidenceProcessingRepository.SetStatusAsync(
                    request.EvidenceId,
                    EvidenceStatusIds.Failed,
                    _settings.EvidenceServiceUserId,
                    statusCts.Token);
            }
            catch (Exception statusEx)
            {
                // Never let the failure-status write mask the original error.
                _logger.LogError(
                    statusEx,
                    "[EVIDENCE-PROCESS] Failed to mark EvidenceId={EvidenceId} as StatusId={StatusId} (Failed) after the original error.",
                    request.EvidenceId,
                    EvidenceStatusIds.Failed);
            }

            throw;
        }
    }

    /// <summary>
    /// Single centralized classifier for exceptions raised by an agent SDK call (Foundry or
    /// Excel). Returns a short reason string when the failure is TRANSIENT (worth retrying),
    /// or null when it isn't - in which case the exception propagates unclassified and the
    /// outer catch treats it as permanent/unexpected. Used as an exception filter so the
    /// original exception's stack trace/type is preserved until classification is certain.
    /// </summary>
    private static string? ClassifyAgentCallFailure(Exception ex, CancellationToken requestToken)
    {
        switch (ex)
        {
            // The SDK's own NetworkTimeout cancelled the call - NOT the caller aborting the
            // ASP.NET request (that check matters: if the request itself was cancelled, the
            // whole response is moot regardless of how we classify it here).
            case OperationCanceledException when !requestToken.IsCancellationRequested:
                return "SDK call timed out (NetworkTimeoutSeconds elapsed).";

            case ClientResultException cre when TransientHttpStatusCodes.Contains(cre.Status):
                return $"agent endpoint returned HTTP {cre.Status}.";

            default:
                return null;
        }
    }

    /// <summary>HTTP statuses from the agent endpoint worth retrying: request/rate-limit timeouts and 5xx.</summary>
    private static readonly HashSet<int> TransientHttpStatusCodes = new() { 408, 429, 500, 502, 503, 504 };

    /// <summary>Truncates diagnostic text to at most <paramref name="maxLength"/> characters, for safe logging of raw agent output.</summary>
    private static string Truncate(string? value, int maxLength) =>
        string.IsNullOrEmpty(value)
            ? "(empty)"
            : value.Length <= maxLength
                ? value
                : value[..maxLength] + "... (truncated)";

    private static void ValidateRequest(ProcessEvidenceRequestDto request)
    {
        var errors = new List<string>();

        if (request.File is null || request.File.Length == 0)
        {
            errors.Add("file is required and must not be empty.");
        }

        if (request.EvidenceId <= 0)
        {
            errors.Add("evidenceId is required and must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.EvidenceType))
        {
            errors.Add("evidenceType is required.");
        }

        if (string.IsNullOrWhiteSpace(request.TlmMetricCode))
        {
            errors.Add("tlmMetricCode is required.");
        }

        if (request.ReportingYear <= 0)
        {
            errors.Add("reportingYear is required and must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.ReportingQuarter))
        {
            errors.Add("reportingQuarter is required.");
        }

        // l1MetricCode is diagnostic only (captured upstream at save time) - not validated here.

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    /// <summary>One row extracted from Foundry output, before matching against EvidenceField definitions.</summary>
    private sealed record ExtractedRow(
        string FieldName,
        string? Value,
        string? LineItemValue,
        bool IsLineItem,
        int PageNumber,
        double? ConfidenceScore);

    /// <summary>Result of <see cref="ParseAndFlatten"/>: the flattened rows plus the root-level values.</summary>
    private sealed record ParseResult(
        IReadOnlyList<ExtractedRow> Rows,
        string? ValidationStatus,
        string? EvidencedValue,
        string? ValidationResult,
        IReadOnlyDictionary<int, (bool LlmIsUnique, int? LlmOriginalPage)> PageMeta,
        string? IsWithinReportingRaw = null);

    /// <summary>
    /// Parses the Foundry agent's nested JSON output_text and flattens it into one row
    /// per scalar field, or one row per line item of a repeating field:
    /// { "documentResults": [ { "pages": [ { "data": [
    ///     { "fieldName", "value", "confidence" }                              -- scalar
    /// ], "lineItems": { "fieldName", "items": [
    ///     { "fieldName", "value", "confidence" }, ... ] } } ] } ],
    ///   "validation": { "validationStatus", "validationComment" },
    ///   "evidenced": { "fieldName", "evidencedValue" }, "unclassifiedPages": [] }
    /// Concatenates data[]/lineItems across ALL documentResults[] and their pages[].
    /// validationStatus/evidencedValue/validationResult are all read from the ROOT
    /// object, not per documentResult.
    /// Throws <see cref="JsonException"/> on malformed JSON syntax, or
    /// <see cref="PermanentProcessingException"/> if the root object is missing
    /// 'documentResults'/'unclassifiedPages' - both are PERMANENT failures the caller
    /// must classify as such, never retried as-is.
    /// </summary>
    private static ParseResult ParseAndFlatten(string outputText)
    {
        var rows = new List<ExtractedRow>();
        var pageMeta = new Dictionary<int, (bool LlmIsUnique, int? LlmOriginalPage)>();
        string? validationStatus = null;
        string? validationResult = null;
        string? evidencedValue = null;
        string? isWithinReportingRaw = null;

        var json = StripJsonFences(outputText);

        if (string.IsNullOrWhiteSpace(json))
        {
            return new ParseResult(rows, validationStatus, evidencedValue, validationResult, pageMeta);
        }

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        // Schema check: a syntactically valid JSON object that is still the wrong SHAPE (e.g. the
        // agent emitted an unrelated object) is just as un-retryable as malformed JSON - classify
        // it PERMANENT here rather than silently proceeding with zero extracted rows.
        if (!root.TryGetProperty("documentResults", out _) || !root.TryGetProperty("unclassifiedPages", out _))
        {
            throw new PermanentProcessingException(
                "Agent output failed schema validation: missing 'documentResults' or 'unclassifiedPages'.");
        }

        if (root.TryGetProperty("validation", out var validationElement) &&
            validationElement.ValueKind == JsonValueKind.Object)
        {
            if (validationElement.TryGetProperty("validationStatus", out var statusElement) &&
                statusElement.ValueKind == JsonValueKind.String)
            {
                validationStatus = statusElement.GetString();
            }

            var validationComment = validationElement.TryGetProperty("validationComment", out var commentElement) &&
                                     commentElement.ValueKind == JsonValueKind.String
                ? commentElement.GetString()
                : null;

            isWithinReportingRaw = validationElement.TryGetProperty("isWithinReporting", out var isWithinReportingElement) &&
                                   isWithinReportingElement.ValueKind == JsonValueKind.String
                ? isWithinReportingElement.GetString()
                : null;

            validationResult = JsonSerializer.Serialize(new
            {
                ValidationStatus = validationStatus,
                ValidationComment = validationComment,
                IsWithinReporting = isWithinReportingRaw
            });
        }

        if (root.TryGetProperty("evidenced", out var evidencedElement) &&
        evidencedElement.ValueKind == JsonValueKind.Object &&
        evidencedElement.TryGetProperty("evidencedValue", out var evidencedValueElement))
        {
            evidencedValue = ElementToString(evidencedValueElement);   // handles string OR number OR null
        }

        if (!root.TryGetProperty("documentResults", out var documentResultsElement) ||
            documentResultsElement.ValueKind != JsonValueKind.Array)
        {
            return new ParseResult(rows, validationStatus, evidencedValue, validationResult, pageMeta, isWithinReportingRaw);
        }

        foreach (var documentResult in documentResultsElement.EnumerateArray())
        {
            if (!documentResult.TryGetProperty("pages", out var pagesElement) ||
                pagesElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var page in pagesElement.EnumerateArray())
            {
                var pageNumber = ReadPageNumber(page);
                pageMeta[pageNumber] = (ReadIsUnique(page), ReadOriginalPage(page));
                AppendScalarRows(page, pageNumber, rows);
                AppendLineItemRows(page, pageNumber, rows);
            }
        }

        return new ParseResult(rows, validationStatus, evidencedValue, validationResult, pageMeta, isWithinReportingRaw);
    }

    /// <summary>Reads the page's 1-based pageNumber, normalizing 0/missing/&lt;1 to 1.</summary>
    private static int ReadPageNumber(JsonElement page) =>
        page.TryGetProperty("pageNumber", out var pageNumberElement) &&
        pageNumberElement.ValueKind == JsonValueKind.Number &&
        pageNumberElement.TryGetInt32(out var pageNumber) &&
        pageNumber >= 1
            ? pageNumber
            : 1;

    /// <summary>
    /// Reads the page's LLM first-pass isUnique guess. Defaults to true (unique) when
    /// missing or unparsable - only an explicit false triggers deterministic confirmation.
    /// </summary>
    private static bool ReadIsUnique(JsonElement page)
    {
        if (!page.TryGetProperty("isUnique", out var element))
        {
            return true;
        }

        return element.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String when bool.TryParse(element.GetString(), out var parsed) => parsed,
            _ => true
        };
    }

    /// <summary>Reads the page's LLM first-pass originalPage guess, if present. Not trusted as-is - only used to decide which pages to deterministically re-check.</summary>
    private static int? ReadOriginalPage(JsonElement page) =>
        page.TryGetProperty("originalPage", out var element) &&
        element.ValueKind == JsonValueKind.Number &&
        element.TryGetInt32(out var originalPage)
            ? originalPage
            : null;

    /// <summary>
    /// Deterministically confirms or overrides the LLM's per-page duplicate guesses by
    /// comparing actual extracted VALUES (not confidence) against every earlier page.
    /// Pages the LLM called unique are trusted as-is; only pages flagged isUnique=false are
    /// checked, and the check can override a false alarm back to unique.
    /// </summary>
    private static Dictionary<int, (bool IsUnique, int? OriginalPage)> ConfirmPageUniqueness(
        IReadOnlyList<ExtractedRow> rows,
        IReadOnlyDictionary<int, (bool LlmIsUnique, int? LlmOriginalPage)> pageMeta)
    {
        var pageNumbers = rows.Select(r => r.PageNumber).Distinct().OrderBy(p => p).ToList();
        var signatures = pageNumbers.ToDictionary(p => p, p => BuildPageSignature(rows, p));
        var result = new Dictionary<int, (bool IsUnique, int? OriginalPage)>();

        foreach (var pageNumber in pageNumbers)
        {
            var llmFlaggedDuplicate = pageMeta.TryGetValue(pageNumber, out var meta) && !meta.LlmIsUnique;

            if (!llmFlaggedDuplicate)
            {
                result[pageNumber] = (true, null);
                continue;
            }

            int? matchedEarlierPage = null;
            foreach (var earlierPage in pageNumbers.Where(p => p < pageNumber))
            {
                if (signatures[earlierPage] == signatures[pageNumber])
                {
                    matchedEarlierPage = earlierPage;
                    break;
                }
            }

            // Confirmed duplicate uses the actual matching earlier page, never the LLM's guess.
            // No earlier page matches -> the LLM's duplicate flag was a false alarm; override to unique.
            result[pageNumber] = matchedEarlierPage.HasValue
                ? (false, matchedEarlierPage)
                : (true, null);
        }

        return result;
    }

    /// <summary>
    /// Builds a stable, order-independent signature of one page's extracted VALUES (scalars +
    /// line items), ignoring confidence, for duplicate-page comparison.
    /// </summary>
    private static string BuildPageSignature(IReadOnlyList<ExtractedRow> rows, int pageNumber)
    {
        var pageRows = rows.Where(r => r.PageNumber == pageNumber).ToList();

        var scalarSignature = pageRows
            .Where(r => !r.IsLineItem)
            .Select(r => $"{NormalizeForSignature(r.FieldName)}={NormalizeForSignature(r.Value)}")
            .OrderBy(s => s, StringComparer.Ordinal);

        var lineItemSignature = pageRows
            .Where(r => r.IsLineItem)
            .Select(r => $"{NormalizeForSignature(r.Value)}={NormalizeForSignature(r.LineItemValue)}")
            .OrderBy(s => s, StringComparer.Ordinal);

        return string.Join("|", scalarSignature) + "||" + string.Join("|", lineItemSignature);
    }

    private static string NormalizeForSignature(string? value) =>
        (value ?? string.Empty).Trim().ToUpperInvariant();

    /// <summary>Appends one row per data[] entry (scalar fields) for a single page.</summary>
    private static void AppendScalarRows(JsonElement page, int pageNumber, List<ExtractedRow> rows)
    {
        if (!page.TryGetProperty("data", out var dataElement) ||
            dataElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var fieldElement in dataElement.EnumerateArray())
        {
            if (!fieldElement.TryGetProperty("fieldName", out var fieldNameElement) ||
                fieldNameElement.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var fieldName = fieldNameElement.GetString();
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                continue;
            }

            var value = fieldElement.TryGetProperty("value", out var valueElement)
                ? ElementToString(valueElement)
                : null;

            var confidence = TryGetConfidence(fieldElement);

            rows.Add(new ExtractedRow(fieldName, value, null, false, pageNumber, confidence));
        }
    }

    /// <summary>
    /// Appends one row per lineItems.items[] entry for a single page. lineItems is a
    /// sibling of data[] on the page, not nested inside it: { "fieldName", "items": [] }.
    /// Skipped entirely when items is empty or fieldName is blank (no line items on this page).
    /// </summary>
    private static void AppendLineItemRows(JsonElement page, int pageNumber, List<ExtractedRow> rows)
    {
        if (!page.TryGetProperty("lineItems", out var lineItemsElement) ||
            lineItemsElement.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (!lineItemsElement.TryGetProperty("fieldName", out var fieldNameElement) ||
            fieldNameElement.ValueKind != JsonValueKind.String)
        {
            return;
        }

        var fieldName = fieldNameElement.GetString();
        if (string.IsNullOrWhiteSpace(fieldName))
        {
            return;
        }

        if (!lineItemsElement.TryGetProperty("items", out var itemsElement) ||
            itemsElement.ValueKind != JsonValueKind.Array ||
            itemsElement.GetArrayLength() == 0)
        {
            return;
        }

        foreach (var item in itemsElement.EnumerateArray())
        {
            var itemName = item.TryGetProperty("fieldName", out var itemNameElement)
                ? ElementToString(itemNameElement)
                : null;

            var itemValue = item.TryGetProperty("value", out var itemValueElement)
                ? ElementToString(itemValueElement)
                : null;

            var itemConfidence = TryGetConfidence(item);

            rows.Add(new ExtractedRow(fieldName, itemName, itemValue, true, pageNumber, itemConfidence));
        }
    }

    private static double? TryGetConfidence(JsonElement element) =>
        element.TryGetProperty("confidence", out var confidenceElement) &&
        confidenceElement.ValueKind == JsonValueKind.Number
            ? confidenceElement.GetDouble()
            : null;

    private static string? ElementToString(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Null => null,
        JsonValueKind.True => "true",
        JsonValueKind.False => "false",
        _ => element.GetRawText()
    };

    /// <summary>Strips a ```json ... ``` (or bare ``` ... ```) fence wrapper, if present.</summary>
    private static string StripJsonFences(string text)
    {
        var trimmed = text.Trim();

        if (!trimmed.StartsWith("```", StringComparison.Ordinal))
        {
            return trimmed;
        }

        var firstNewline = trimmed.IndexOf('\n');
        if (firstNewline < 0)
        {
            return trimmed;
        }

        trimmed = trimmed[(firstNewline + 1)..];

        var closingFenceIndex = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        if (closingFenceIndex >= 0)
        {
            trimmed = trimmed[..closingFenceIndex];
        }

        return trimmed.Trim();
    }

    /// <summary>
    /// Keeps only the rows whose key matches a defined EvidenceKey, matching
    /// normalization-tolerantly (uppercase, non-alphanumeric characters stripped from
    /// both sides). The line-item parent key is a single stable value (not indexed), so
    /// no suffix stripping is needed. Rows with no matching definition are dropped.
    /// </summary>
    private static IReadOnlyList<EvidenceDetailDto> FilterToDefinedFields(
        IReadOnlyList<ExtractedRow> rows,
        IReadOnlyList<EvidenceFieldDefinition> fields,
        IReadOnlyDictionary<int, (bool IsUnique, int? OriginalPage)> pageUniqueness)
    {
        var fieldsByNormalizedKey = new Dictionary<string, EvidenceFieldDefinition>();

        foreach (var field in fields)
        {
            fieldsByNormalizedKey[Normalize(field.EvidenceKey)] = field;
        }

        var details = new List<EvidenceDetailDto>();

        foreach (var row in rows)
        {
            if (fieldsByNormalizedKey.TryGetValue(Normalize(row.FieldName), out var field))
            {
                var (isUnique, originalPage) = pageUniqueness.TryGetValue(row.PageNumber, out var verified)
                    ? verified
                    : (true, null);

                details.Add(new EvidenceDetailDto
                {
                    EvidenceFieldId = field.EvidenceFieldId,
                    Value = row.Value,
                    LineItemValue = row.LineItemValue,
                    IsLineItem = row.IsLineItem,
                    PageNumber = row.PageNumber,
                    IsUnique = isUnique,
                    OriginalPage = originalPage,
                    ConfidenceScore = row.ConfidenceScore
                });
            }
        }

        return details;
    }

    /// <summary>Uppercases and strips all non-alphanumeric characters for tolerant key matching.</summary>
    private static string Normalize(string key) =>
        new(key.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    /// <summary>Divisor applied to the raw USD total to produce the USD-millions evidenced value.</summary>
    private const decimal EvidencedValueDivisor = 1_000_000m;
    /// <summary>Decimal places used when rounding the evidenced value (in millions).</summary>
    private const int EvidencedValueRoundingDecimals = 2;
    /// <summary>Average confidence below this threshold forces IsReviewRequired=true regardless of validationStatus.</summary>
    private const double ConfidenceReviewThreshold = 0.80;

    /// <summary>Frankfurter's supported currency set - exactly this list, nothing added.</summary>
    private static readonly HashSet<string> AllowedFxCurrencies = new(StringComparer.Ordinal)
    {
        "AUD", "BRL", "CAD", "CHF", "CNY", "CZK", "DKK", "EUR", "GBP", "HKD", "HUF", "IDR", "ILS",
        "INR", "ISK", "JPY", "KRW", "MXN", "MYR", "NOK", "NZD", "PHP", "PLN", "RON", "SEK", "SGD",
        "THB", "TRY", "USD", "ZAR"
    };

    private static readonly string[] FxDateFormats =
    [
        "yyyy-MM-dd", "yyyy/MM/dd",
        "d MMMM yyyy", "d MMM yyyy",                      // 1 February 2025 / 1 Feb 2025
        "MMMM d yyyy", "MMM d yyyy",                      // February 1 2025 (comma stripped first)
        "dd.MM.yyyy", "d.M.yyyy", "dd.MM.yy", "d.M.yy",
        "dd/MM/yyyy", "d/M/yyyy", "dd/MM/yy", "d/M/yy",   // dd/MM first - non-US default
        "MM/dd/yyyy", "M/d/yyyy", "MM/dd/yy", "M/d/yy",   // US fallback
        "dd-MM-yyyy", "d-M-yyyy",
        "yyyyMMdd"
    ];

    private static readonly Regex FxDateOrdinalSuffixRegex = new(@"(?<=\d)(st|nd|rd|th)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex FxWhitespaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex FxAmountAllowedCharsRegex = new(@"[^0-9.,\-]", RegexOptions.Compiled);

    /// <summary>
    /// Processes every currency entry across all scalar pages, converting each to USD and
    /// accumulating a cross-page total. Also mutates the parsed JSON (from
    /// <paramref name="rawOutputText"/>) so that the returned <c>ModifiedJson</c> carries the
    /// same FX numbers already written to <c>details</c> — EvidenceRawExtraction, EvidenceDetail,
    /// and Evidence.EvidencedValue then all agree. USD-source entries skip the exchange-rate
    /// endpoint (rate=1). Returns <c>(null, strippedJson)</c> when no page produced a value.
    /// </summary>
    private async Task<(string? EvidencedValue, string ModifiedJson)> ApplyFxConversionAsync(
        List<EvidenceDetailDto> details,
        IReadOnlyList<EvidenceFieldDefinition> fields,
        IReadOnlyDictionary<int, (bool IsUnique, int? OriginalPage)> pageUniqueness,
        string rawOutputText,
        CancellationToken cancellationToken)
    {
        decimal totalUsd = 0m;
        var convertedPageCount = 0;

        // Parse the agent JSON into a mutable node tree so FX values can be written back
        // into the same structure before it is saved to EvidenceRawExtraction.
        var strippedJson = StripJsonFences(rawOutputText);
        JsonNode? jsonRoot = null;
        var jsonPagesByEffectiveNumber = new Dictionary<int, JsonObject>();

        try
        {
            jsonRoot = JsonNode.Parse(strippedJson);

            if (jsonRoot?["documentResults"] is JsonArray docResults)
            {
                foreach (var docResult in docResults.OfType<JsonObject>())
                {
                    if (docResult["pages"] is not JsonArray pages) continue;

                    foreach (var pageNode in pages.OfType<JsonObject>())
                    {
                        var effectivePage = 1;
                        if (pageNode["pageNumber"] is JsonValue pnVal &&
                            pnVal.TryGetValue(out int pn) && pn >= 1)
                        {
                            effectivePage = pn;
                        }
                        jsonPagesByEffectiveNumber[effectivePage] = pageNode;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "[EVIDENCE-PROCESS][STEP 6b][FX] Could not parse agent JSON into mutable node; FX values will be written to EvidenceDetail only, not to RawExtraction.");
        }

        var fieldsByNormalizedKey = new Dictionary<string, EvidenceFieldDefinition>();

        foreach (var field in fields)
        {
            fieldsByNormalizedKey[Normalize(field.EvidenceKey)] = field;
        }

        if (!fieldsByNormalizedKey.TryGetValue(Normalize("Source Currency"), out var sourceCurrencyField) ||
            !fieldsByNormalizedKey.TryGetValue(Normalize("Amount"), out var amountField) ||
            !fieldsByNormalizedKey.TryGetValue(Normalize("Date"), out var dateField))
        {
            _logger.LogWarning(
                "[EVIDENCE-PROCESS][STEP 6b][FX] IsExchangeRateRequired=true but 'Source Currency'/'Amount'/'Date' are not all defined as EvidenceFields for this EvidenceType - skipping FX conversion entirely. EvidenceKeys=[{EvidenceKeys}]",
                string.Join(", ", fields.Select(f => f.EvidenceKey)));

            return (null, strippedJson);
        }

        // Materialized (not lazy) - the loop body mutates `details` via UpsertFxDetail, which
        // would otherwise invalidate an in-flight deferred LINQ enumeration over it.
        var pageNumbers = details
            .Where(d => !d.IsLineItem)
            .Select(d => d.PageNumber)
            .Distinct()
            .ToList();

        if (pageNumbers.Count == 0)
        {
            _logger.LogWarning(
                "[EVIDENCE-PROCESS][STEP 6b][FX] IsExchangeRateRequired=true and FX fields are defined, but 0 EvidenceDetail rows were filtered through in STEP 6 - nothing to convert. Check STEP 6's 'UnmatchedFieldNames' warning.");

            return (null, strippedJson);
        }

        foreach (var pageNumber in pageNumbers)
        {
            var pageRows = details.Where(d => !d.IsLineItem && d.PageNumber == pageNumber).ToList();

            var sourceCurrencyRow = pageRows.FirstOrDefault(d => d.EvidenceFieldId == sourceCurrencyField.EvidenceFieldId);

            if (sourceCurrencyRow is null)
            {
                continue;
            }

            var amountRow = pageRows.FirstOrDefault(d => d.EvidenceFieldId == amountField.EvidenceFieldId);
            var dateRow   = pageRows.FirstOrDefault(d => d.EvidenceFieldId == dateField.EvidenceFieldId);

            var sourceCurrency = sourceCurrencyRow.Value?.Trim().ToUpperInvariant() ?? string.Empty;

            var amount = ParseFlexibleAmount(amountRow?.Value);
            if (amount is null)
            {
                _logger.LogInformation(
                    "[EVIDENCE-PROCESS][STEP 6b][FX] Page {PageNumber}: could not parse amount '{Amount}'; skipping.",
                    pageNumber,
                    amountRow?.Value);
                continue;
            }

            decimal pageUsd;
            string  exchangeRateValue;
            string  rateDateValue;

            if (sourceCurrency == "USD")
            {
                // USD source: amount is already in USD; no API call needed.
                pageUsd           = amount.Value;
                exchangeRateValue = "1";
                rateDateValue     = dateRow?.Value ?? "N/A";

                _logger.LogInformation(
                    "[EVIDENCE-PROCESS][STEP 6b][FX] Page {PageNumber}: Source Currency=USD; using amount directly, no Frankfurter call. Amount={Amount}.",
                    pageNumber,
                    amount.Value);
            }
            else
            {
                if (!AllowedFxCurrencies.Contains(sourceCurrency))
                {
                    _logger.LogInformation(
                        "[EVIDENCE-PROCESS][STEP 6b][FX] Page {PageNumber}: currency NA/unsupported '{Currency}'; left for manual entry.",
                        pageNumber,
                        sourceCurrencyRow.Value ?? "(empty)");
                    continue;
                }

                var date = ParseFlexibleDate(dateRow?.Value);
                if (date is null)
                {
                    _logger.LogInformation(
                        "[EVIDENCE-PROCESS][STEP 6b][FX] Page {PageNumber}: could not parse date '{Date}'; skipping conversion.",
                        pageNumber,
                        dateRow?.Value);
                    continue;
                }

                _logger.LogInformation(
                    "[EVIDENCE-PROCESS][STEP 6b][FX] Page {PageNumber}: parsed amount '{Raw}' -> {Amount}, date -> {Date:yyyy-MM-dd}. Calling Frankfurter for {Currency}...",
                    pageNumber,
                    amountRow?.Value,
                    amount.Value,
                    date.Value,
                    sourceCurrency);

                var rateResult = await _exchangeRateService.GetRateToUsdAsync(sourceCurrency, date.Value, cancellationToken);
                if (rateResult is null)
                {
                    _logger.LogWarning(
                        "[EVIDENCE-PROCESS][STEP 6b][FX] Page {PageNumber}: Frankfurter rate unavailable for {Currency} on {Date}; left for manual entry.",
                        pageNumber,
                        sourceCurrency,
                        date.Value);
                    continue;
                }

                var (rate, rateDate) = rateResult.Value;
                pageUsd           = amount.Value * rate;
                exchangeRateValue = rate.ToString(CultureInfo.InvariantCulture);
                rateDateValue     = rateDate.ToString("yyyy-MM-dd");

                _logger.LogInformation(
                    "[EVIDENCE-PROCESS][STEP 6b][FX] Page {PageNumber}: converted {Amount} {Currency} -> {PageUsd} USD (rate {Rate} effective {RateDate}).",
                    pageNumber,
                    amount.Value,
                    sourceCurrency,
                    pageUsd,
                    rate,
                    rateDate);
            }

            var (isUnique, originalPage) = pageUniqueness.TryGetValue(pageNumber, out var verified)
                ? verified
                : (true, null);

            // Per-page "USD Converted Amount" is stored in absolute USD (not millions).
            var usdAmountAbsoluteStr = pageUsd.ToString(CultureInfo.InvariantCulture);

            UpsertFxDetail(details, fieldsByNormalizedKey, "ExchangeRate",         pageNumber, exchangeRateValue,     isUnique, originalPage);
            UpsertFxDetail(details, fieldsByNormalizedKey, "Rate Date",             pageNumber, rateDateValue,         isUnique, originalPage);
            UpsertFxDetail(details, fieldsByNormalizedKey, "USD Converted Amount",  pageNumber, usdAmountAbsoluteStr, isUnique, originalPage);

            // Mirror the same values into the JSON node so RawExtraction is consistent.
            if (jsonPagesByEffectiveNumber.TryGetValue(pageNumber, out var jsonPage))
            {
                SetFxFieldsOnPage(jsonPage, exchangeRateValue, rateDateValue, usdAmountAbsoluteStr);
            }

            _logger.LogInformation(
                "[EVIDENCE-PROCESS][STEP 6b][FX] Page {PageNumber}: filled - ExchangeRate={ExchangeRate}, RateDate={RateDate}, USDConvertedAmount={UsdAmount} (absolute USD).",
                pageNumber,
                exchangeRateValue,
                rateDateValue,
                usdAmountAbsoluteStr);

            totalUsd += pageUsd;
            convertedPageCount++;
        }

        if (convertedPageCount == 0)
        {
            return (null, strippedJson);
        }

        var evidencedValueInMillions = Math.Round(totalUsd / EvidencedValueDivisor, EvidencedValueRoundingDecimals, MidpointRounding.AwayFromZero);
        var evidencedValueStr = evidencedValueInMillions.ToString(CultureInfo.InvariantCulture);

        // Write the aggregated evidenced value into the JSON so all three stores agree.
        if (jsonRoot?["evidenced"] is JsonObject evidencedObj)
        {
            evidencedObj["evidencedValue"] = evidencedValueStr;
        }

        _logger.LogInformation(
            "[EVIDENCE-PROCESS][STEP 6b][FX] Aggregated {ConvertedPages}/{TotalPages} page(s): totalUSD={TotalUsd}, evidencedValue={EvidencedValue} USD millions.",
            convertedPageCount,
            pageNumbers.Count,
            totalUsd,
            evidencedValueInMillions);

        var modifiedJson = jsonRoot is not null ? jsonRoot.ToJsonString() : strippedJson;
        return (evidencedValueStr, modifiedJson);
    }

    /// <summary>
    /// Writes (or adds) ExchangeRate, Rate Date, and USD Converted Amount into the mutable
    /// data[] array of one page node. Sets confidence=1 to signal computed (not extracted).
    /// </summary>
    private static void SetFxFieldsOnPage(JsonObject page, string exchangeRate, string rateDate, string usdConverted)
    {
        if (page["data"] is not JsonArray data)
        {
            data = [];
            page["data"] = data;
        }

        SetOrAddDataField(data, "ExchangeRate",         exchangeRate);
        SetOrAddDataField(data, "Rate Date",             rateDate);
        SetOrAddDataField(data, "USD Converted Amount",  usdConverted);
    }

    /// <summary>
    /// Finds the data[] entry whose fieldName matches (case-insensitive) and updates its value
    /// and confidence. Adds a new entry with confidence=1 if none exists.
    /// </summary>
    private static void SetOrAddDataField(JsonArray data, string fieldName, string value)
    {
        foreach (var item in data)
        {
            if (item is not JsonObject obj) continue;

            if (obj["fieldName"] is JsonValue fnVal &&
                fnVal.TryGetValue(out string? fn) &&
                string.Equals(fn, fieldName, StringComparison.OrdinalIgnoreCase))
            {
                obj["value"]      = value;
                obj["confidence"] = 1.0;
                return;
            }
        }

        data.Add(new JsonObject
        {
            ["fieldName"]  = fieldName,
            ["value"]      = value,
            ["confidence"] = 1.0
        });
    }

    /// <summary>
    /// Replaces (or adds) the EvidenceDetail row for an API-computed FX field on one page -
    /// this is the point where the agent-returned/filtered data is modified with the FX
    /// conversion result before STEP 7 persists it to the EvidenceDetail table.
    /// </summary>
    private void UpsertFxDetail(
        List<EvidenceDetailDto> details,
        IReadOnlyDictionary<string, EvidenceFieldDefinition> fieldsByNormalizedKey,
        string keyName,
        int pageNumber,
        string value,
        bool isUnique,
        int? originalPage)
    {
        if (!fieldsByNormalizedKey.TryGetValue(Normalize(keyName), out var field))
        {
            _logger.LogInformation(
                "[EVIDENCE-PROCESS][STEP 6b][FX] Page {PageNumber}: EvidenceField '{KeyName}' is not defined for this EvidenceType; skipping fill.",
                pageNumber,
                keyName);

            return;
        }

        var existing = details.FirstOrDefault(d =>
            !d.IsLineItem && d.PageNumber == pageNumber && d.EvidenceFieldId == field.EvidenceFieldId);

        _logger.LogInformation(
            "[EVIDENCE-PROCESS][STEP 6b][FX] Page {PageNumber}: {Action} EvidenceDetail - EvidenceFieldId={EvidenceFieldId} ('{KeyName}'), OldValue={OldValue}, NewValue={NewValue}.",
            pageNumber,
            existing is null ? "Adding" : "Replacing",
            field.EvidenceFieldId,
            keyName,
            existing?.Value ?? "(none)",
            value);

        details.RemoveAll(d => !d.IsLineItem && d.PageNumber == pageNumber && d.EvidenceFieldId == field.EvidenceFieldId);

        details.Add(new EvidenceDetailDto
        {
            EvidenceFieldId = field.EvidenceFieldId,
            Value = value,
            LineItemValue = null,
            IsLineItem = false,
            PageNumber = pageNumber,
            IsUnique = isUnique,
            OriginalPage = originalPage,
            ConfidenceScore = 1.0
        });
    }

    /// <summary>
    /// Locale-tolerant decimal parse of OCR'd/agent-extracted amount text. Strips currency
    /// symbols, codes, spaces, and text prefixes (keeping only digits, '.', ',', and a leading
    /// '-'), then decides which of '.'/',' is the decimal separator by position/occurrence count
    /// before normalizing to invariant-culture decimal syntax. Handles both US/UK format
    /// (comma=thousands, period=decimal, e.g. "9,546,364.00") and European/BR format
    /// (period=thousands, comma=decimal, e.g. "9.546.364,00"). Used ONLY for the FX numeric
    /// conversion - the stored/displayed Amount value and raw agent JSON are never touched.
    /// Returns null (skip conversion for that page) when the result isn't a valid, non-zero
    /// decimal - never throws.
    /// </summary>
    internal static decimal? ParseFlexibleAmount(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var stripped = FxAmountAllowedCharsRegex.Replace(raw.Trim(), string.Empty);
        var isNegative = stripped.StartsWith('-');
        var digits = stripped.Replace("-", string.Empty);

        if (digits.Length == 0)
        {
            return null;
        }

        var lastDotIndex = digits.LastIndexOf('.');
        var lastCommaIndex = digits.LastIndexOf(',');
        var dotCount = digits.Count(c => c == '.');
        var commaCount = digits.Count(c => c == ',');

        string normalized;

        if (lastDotIndex >= 0 && lastCommaIndex >= 0)
        {
            // Both present - whichever appears LAST (highest index) is the decimal separator.
            normalized = lastCommaIndex > lastDotIndex
                ? digits.Replace(".", string.Empty).Replace(',', '.')   // comma=decimal, dot=thousands
                : digits.Replace(",", string.Empty);                    // dot=decimal, comma=thousands
        }
        else if (commaCount > 0)
        {
            // Only comma present.
            if (commaCount > 1)
            {
                normalized = digits.Replace(",", string.Empty); // all thousands
            }
            else
            {
                var digitsAfter = digits.Length - lastCommaIndex - 1;
                normalized = digitsAfter is 1 or 2
                    ? digits.Replace(',', '.')          // decimal
                    : digits.Replace(",", string.Empty); // thousands
            }
        }
        else if (dotCount > 0)
        {
            // Only dot present - symmetric to the comma-only case.
            if (dotCount > 1)
            {
                normalized = digits.Replace(".", string.Empty); // all thousands
            }
            else
            {
                var digitsAfter = digits.Length - lastDotIndex - 1;
                normalized = digitsAfter is 1 or 2
                    ? digits                              // already decimal-formatted
                    : digits.Replace(".", string.Empty);   // thousands
            }
        }
        else
        {
            // Neither separator present - plain digits.
            normalized = digits;
        }

        var candidate = isNegative ? "-" + normalized : normalized;

        if (!decimal.TryParse(
                candidate,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var amount))
        {
            return null;
        }

        return amount == 0 ? null : amount;
    }

    /// <summary>
    /// Tolerant date parse of OCR'd transaction-date text: strips ordinal suffixes ("1st" -> "1"),
    /// collapses whitespace, then tries the explicit, unambiguous format list FIRST. A generic
    /// DateTime.TryParse only runs as a last resort, since invariant-culture parsing is lenient
    /// about separators and will silently misread e.g. "02.12.2024" (dd.MM.yyyy) as February 12th
    /// rather than failing.
    /// </summary>
    private static DateOnly? ParseFlexibleDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        var trimmed = FxDateOrdinalSuffixRegex.Replace(raw.Trim(), string.Empty);
        trimmed = FxWhitespaceRegex.Replace(trimmed, " ").Trim();

        var withoutCommas = trimmed.Replace(",", string.Empty);

        if (DateTime.TryParseExact(withoutCommas, FxDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
        {
            return DateOnly.FromDateTime(exact);
        }

        return DateTime.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None, out var loose)
            ? DateOnly.FromDateTime(loose)
            : null;
    }

    /// <summary>
    /// Maps Foundry's validationStatus to the review flag. Confirmed vocabulary:
    /// "Processed" / "Not Required" -> not required; "Pending" / "Failed" -> required;
    /// anything else (including unknown/missing) -> required.
    /// </summary>
    private static bool DetermineIsReviewRequired(string? validationStatus)
    {
        if (string.IsNullOrWhiteSpace(validationStatus))
        {
            return true;
        }

        var trimmed = validationStatus.Trim();

        return !(trimmed.Equals("Processed", StringComparison.OrdinalIgnoreCase) ||
                 trimmed.Equals("Not Required", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Maps the agent's validation.isWithinReporting string to Evidence.IsWithinReporting.
    /// Single source of truth for this mapping - case-insensitive, trimmed. Missing/null/blank
    /// defaults to true (benefit of the doubt, matches the column's own DEFAULT).
    /// </summary>
    private static bool MapIsWithinReporting(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        return raw.Trim().ToLowerInvariant() switch
        {
            "true" or "yes" or "1" or "not required" => true,
            _ => false
        };
    }
}
