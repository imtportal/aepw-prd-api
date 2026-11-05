-- =============================================================================
-- Change            : EvidenceType.IsExchangeRateRequired
-- Description        : Explicit, data-driven flag that gates FX (Frankfurter) conversion
--                      for this evidence type, replacing the old "detect currency fields
--                      in the agent output" trigger. Idempotent - safe to re-run.
-- =============================================================================
IF NOT EXISTS (
    SELECT 1
    FROM   sys.columns
    WHERE  object_id = OBJECT_ID('dbo.EvidenceType')
      AND  name       = 'IsExchangeRateRequired'
)
BEGIN
    ALTER TABLE dbo.EvidenceType
        ADD IsExchangeRateRequired BIT NOT NULL CONSTRAINT DF_EvidenceType_IsExchangeRateRequired DEFAULT (0);
END
GO
