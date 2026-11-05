-- =============================================================================
-- Table             : EvidenceRawExtraction
-- Description        : One row per Evidence, holding the raw Document Intelligence
--                      model response. Overwritten (not appended) on reprocess.
-- =============================================================================
IF NOT EXISTS (
    SELECT 1
    FROM   sys.objects
    WHERE  object_id = OBJECT_ID('dbo.EvidenceRawExtraction')
      AND  type       = 'U'
)
BEGIN
    CREATE TABLE dbo.EvidenceRawExtraction
    (
        EvidenceId   BIGINT          NOT NULL,
        RawJson      NVARCHAR(MAX)   NOT NULL,
        ModelId      NVARCHAR(100)   NULL,
        ProcessedUtc DATETIME2       NOT NULL CONSTRAINT DF_EvidenceRawExtraction_ProcessedUtc DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_EvidenceRawExtraction PRIMARY KEY CLUSTERED (EvidenceId),
        CONSTRAINT FK_EvidenceRawExtraction_Evidence FOREIGN KEY (EvidenceId)
            REFERENCES dbo.Evidence (EvidenceId)
    );
END
GO
