-- =============================================================================
-- Stored Procedure : dbo.uspEvidenceSaveRawExtraction
-- Type             : CUD (upsert)
-- Description      : Upserts the raw Document Intelligence model response for an
--                    EvidenceId. Overwrites on reprocess (one row per EvidenceId).
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.uspEvidenceSaveRawExtraction
    @EvidenceId BIGINT,
    @RawJson    NVARCHAR(MAX),
    @ModelId    NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        MERGE dbo.EvidenceRawExtraction AS target
        USING (SELECT @EvidenceId AS EvidenceId) AS source
            ON target.EvidenceId = source.EvidenceId
        WHEN MATCHED THEN
            UPDATE SET
                RawJson      = @RawJson,
                ModelId      = @ModelId,
                ProcessedUtc = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN
            INSERT (EvidenceId, RawJson, ModelId, ProcessedUtc)
            VALUES (@EvidenceId, @RawJson, @ModelId, SYSUTCDATETIME());
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
