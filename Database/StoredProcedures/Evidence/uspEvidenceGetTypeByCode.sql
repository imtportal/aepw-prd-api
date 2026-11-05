-- =============================================================================
-- Stored Procedure : dbo.uspEvidenceGetTypeByCode
-- Type             : READ
-- Description      : Resolves a dbo.EvidenceType row by its Code. Returns one row
--                    (EvidenceTypeId, ModelId, IsExchangeRateRequired) or no rows when
--                    not found / inactive. NOT metric-scoped - Code is the sole lookup key.
-- Confirmed against the actual deployed procedure 2026-10-21.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.uspEvidenceGetTypeByCode
    @Code NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        et.EvidenceTypeId,
        et.ModelId,
        et.IsExchangeRateRequired          -- 1 = call FX service, 0 = skip
    FROM dbo.EvidenceType et
    WHERE et.Code     = @Code
      AND et.IsActive = 1;
END;
GO
