-- =============================================================================
-- Stored Procedure : dbo.uspEvidenceGetReviewIdByEvidenceId
-- Type             : READ
-- Description      : Resolves the owning ReviewId for an Evidence row, via its
--                    ReviewMetric. Returns one row, or no rows when not found.
-- NOTE: reconcile this script against the actual deployed schema - not confirmed
-- against the live Azure SQL dev DB.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.uspEvidenceGetReviewIdByEvidenceId
    @EvidenceId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT rm.ReviewId
    FROM dbo.Evidence e
    JOIN dbo.ReviewMetric rm ON rm.ReviewMetricId = e.ReviewMetricId
    WHERE e.EvidenceId = @EvidenceId;
END
GO
