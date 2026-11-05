-- =============================================================================
-- Stored Procedure : dbo.uspEvidenceGetReviewDetails
-- Type             : READ
-- Description      : Returns everything the Angular "evidence-review" page needs for
--                    one Evidence row, as four result sets:
--                      1. Header row (Evidence + joined ReviewMetric/Review/ReviewPeriod/
--                         ImpactMaster/Metric/EvidenceType/AppUser/MasterStatus).
--                      2. Scalar field values (EvidenceField LEFT JOIN EvidenceDetail,
--                         IsLineItem = 0) - defined-but-not-extracted fields appear with
--                         a NULL Value.
--                      3. Line items (EvidenceDetail WHERE IsLineItem = 1).
--                      4. Raw agent JSON (EvidenceRawExtraction.RawJson).
--                    If result set 1 has no row, the EvidenceId does not exist (the
--                    repository returns null -> service throws NotFoundException).
-- NOTE: reconcile this script against the actual deployed schema - the joins below
-- (ReviewMetric.MetricId, Review.ImpactMasterId/ReviewPeriodId, ImpactMaster.Pin/Title,
-- Metric.Code, MetricEvidenceType.MetricId/EvidenceTypeId, EvidenceType.Code/Description,
-- AppUser.DisplayName, MasterStatus.StatusCode/StatusName for Module='EVIDENCE') are
-- assumed from other DTOs/repositories in this codebase, not confirmed against the live
-- Azure SQL dev DB.
-- PRECONDITION: EvidenceDetail.PageNumber (multipage migration) must exist - RS2/RS3 select
-- it directly with no fallback. If it is missing in this environment, run that migration first.
-- PRECONDITION: EvidenceDetail.IsUnique/OriginalPage (hybrid page-duplicate detection) must
-- also exist - RS2/RS3 select them directly with no fallback.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.uspEvidenceGetReviewDetails
    @EvidenceId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ReviewMetricId BIGINT;
    DECLARE @EvidenceTypeId BIGINT;
    DECLARE @MetricId BIGINT;
    DECLARE @MetricEvidenceTypeId BIGINT;

    SELECT
        @ReviewMetricId = e.ReviewMetricId,
        @EvidenceTypeId = e.EvidenceTypeId
    FROM dbo.Evidence e
    WHERE e.EvidenceId = @EvidenceId;

    SELECT @MetricId = rm.MetricId
    FROM dbo.ReviewMetric rm
    WHERE rm.ReviewMetricId = @ReviewMetricId;

    IF @EvidenceTypeId IS NOT NULL
    BEGIN
        -- Normal path: Evidence.EvidenceTypeId is set.
        SELECT @MetricEvidenceTypeId = met.MetricEvidenceTypeId
        FROM dbo.MetricEvidenceType met
        WHERE met.MetricId = @MetricId
          AND met.EvidenceTypeId = @EvidenceTypeId;
    END
    ELSE
    BEGIN
        -- Fallback for older rows with no Evidence.EvidenceTypeId: derive the
        -- MetricEvidenceTypeId from the evidence's own already-saved details.
        SELECT TOP (1) @MetricEvidenceTypeId = ef.MetricEvidenceTypeId
        FROM dbo.EvidenceDetail ed
        JOIN dbo.EvidenceField ef ON ef.EvidenceFieldId = ed.EvidenceFieldId
        WHERE ed.EvidenceId = @EvidenceId;
    END

    -- Result set 1: header.
    SELECT
        e.ReviewMetricId,
        e.FileName,
        e.FileSizeBytes,
        e.FileUriView,
        e.FileUriDownload,
        e.UploadedByUserId,
        au.DisplayName       AS UploadedByName,
        e.UploadedUtc,
        e.Status,
        ms.StatusCode        AS StatusCode,
        ms.StatusName        AS StatusName,
        e.EvidencedValue,
        e.ValidationResult,
        rp.PeriodCode        AS ReportingPeriod,
        im.Pin               AS ProjectPin,
        im.Title             AS ProjectTitle,
        mt.Code              AS MetricCode,
        et.Code              AS EvidenceTypeCode,
        et.Description       AS EvidenceTypeDescription
    FROM dbo.Evidence e
    LEFT JOIN dbo.ReviewMetric rm  ON rm.ReviewMetricId = e.ReviewMetricId
    LEFT JOIN dbo.Review rv        ON rv.ReviewId = rm.ReviewId
    LEFT JOIN dbo.ReviewPeriod rp  ON rp.ReviewPeriodId = rv.ReviewPeriodId
    LEFT JOIN dbo.ImpactMaster im  ON im.ImpactMasterId = rv.ImpactMasterId
    LEFT JOIN dbo.Metric mt        ON mt.MetricId = rm.MetricId
    LEFT JOIN dbo.EvidenceType et  ON et.EvidenceTypeId = e.EvidenceTypeId
    LEFT JOIN dbo.AppUser au       ON au.UserId = e.UploadedByUserId
    LEFT JOIN dbo.MasterStatus ms  ON ms.StatusId = e.Status AND ms.Module = 'EVIDENCE'
    WHERE e.EvidenceId = @EvidenceId;

    -- Result set 2: scalar field values (defined fields, left-joined to any extracted value).
    SELECT
        ef.EvidenceFieldId,
        ed.EvidenceDetailId  AS EvidenceDetailId,
        ef.EvidenceKey,
        ed.Value,
        ed.ConfidenceScore,
        ed.PageNumber        AS PageNumber,
        ed.IsUnique          AS IsUnique,
        ed.OriginalPage      AS OriginalPage
    FROM dbo.EvidenceField ef
    LEFT JOIN dbo.EvidenceDetail ed
        ON ed.EvidenceFieldId = ef.EvidenceFieldId
       AND ed.EvidenceId      = @EvidenceId
       AND ed.IsLineItem      = 0
    WHERE ef.MetricEvidenceTypeId = @MetricEvidenceTypeId;

    -- Result set 3: line items.
    SELECT
        ed.EvidenceDetailId AS EvidenceDetailId,
        ed.EvidenceFieldId  AS EvidenceFieldId,
        ef.EvidenceKey     AS FieldName,
        ed.Value           AS Name,
        ed.LineItemValue   AS Value,
        ed.ConfidenceScore AS Confidence,
        ed.PageNumber      AS PageNumber,
        ed.IsUnique        AS IsUnique,
        ed.OriginalPage    AS OriginalPage
    FROM dbo.EvidenceDetail ed
    JOIN dbo.EvidenceField ef ON ef.EvidenceFieldId = ed.EvidenceFieldId
    WHERE ed.EvidenceId = @EvidenceId
      AND ed.IsLineItem = 1;

    -- Result set 4: raw agent JSON.
    SELECT
        er.RawJson
    FROM dbo.EvidenceRawExtraction er
    WHERE er.EvidenceId = @EvidenceId;
END
GO
