-- =============================================================================
-- Stored Procedure : dbo.uspEvidenceGetMetricEvidenceTypeIdByEvidenceId
-- Type             : READ
-- Description      : Resolves the MetricEvidenceTypeId (the Metric+EvidenceType junction row)
--                    for an Evidence row. @EvidenceTypeId is passed in by the caller (already
--                    resolved via uspEvidenceGetTypeByCode) rather than read from
--                    Evidence.EvidenceTypeId, which is NOT reliably populated at process-time.
-- CORRECTED 2026-10-23 (v3): v2's single CASE-based climb (L1 -> its direct ParentMetricId)
-- assumed an exactly-two-level tree (TLM -> L1) and failed for EvidenceId 361 (EvidenceTypeId
-- 136), whose own metric sits more than one level below its TLM. Replaced with a recursive
-- climb that walks ParentMetricId upward until IsTopLevel = 1, regardless of depth - a strict
-- generalization of v2, so the already-working two-level case (e.g. EvidenceId 312, L1 28 ->
-- TLM 4) still resolves the same way.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.uspEvidenceGetMetricEvidenceTypeIdByEvidenceId
    @EvidenceId BIGINT,
    @EvidenceTypeId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @MetricId BIGINT;

    SELECT @MetricId = rm.MetricId
    FROM dbo.Evidence e
    INNER JOIN dbo.ReviewMetric rm ON rm.ReviewMetricId = e.ReviewMetricId
    WHERE e.EvidenceId = @EvidenceId;

    ;WITH MetricAncestry AS (
        SELECT MetricId, ParentMetricId, IsTopLevel, 0 AS Depth
        FROM dbo.Metric
        WHERE MetricId = @MetricId

        UNION ALL

        SELECT m.MetricId, m.ParentMetricId, m.IsTopLevel, ma.Depth + 1
        FROM dbo.Metric m
        INNER JOIN MetricAncestry ma ON m.MetricId = ma.ParentMetricId
        WHERE ma.IsTopLevel = 0
          AND ma.Depth < 10          -- safety cap against a malformed/circular tree
    )
    SELECT TOP (1)
        met.Id AS MetricEvidenceTypeId          -- junction PK is 'Id', not 'MetricEvidenceTypeId'
    FROM MetricAncestry ma
    INNER JOIN dbo.MetricEvidenceType met
            ON met.MetricId = ma.MetricId
           AND met.EvidenceTypeId = @EvidenceTypeId
    WHERE ma.IsTopLevel = 1
    OPTION (MAXRECURSION 10);
END
GO
