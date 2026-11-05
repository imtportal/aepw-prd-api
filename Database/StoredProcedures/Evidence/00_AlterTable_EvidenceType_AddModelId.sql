-- =============================================================================
-- Change            : EvidenceType.ModelId
-- Description        : Adds the Document Intelligence model id used to process
--                      evidence of this type. Idempotent - safe to re-run.
-- =============================================================================
IF NOT EXISTS (
    SELECT 1
    FROM   sys.columns
    WHERE  object_id = OBJECT_ID('dbo.EvidenceType')
      AND  name       = 'ModelId'
)
BEGIN
    ALTER TABLE dbo.EvidenceType
        ADD ModelId NVARCHAR(100) NULL;
END
GO
