-- =============================================================================
-- Stored Procedure : dbo.uspEvidenceGetFieldsByTypeId
-- Type             : READ
-- Description      : Returns the pre-seeded field definitions for a MetricEvidenceType.
--                    RULE: this pipeline only reads EvidenceField rows, never creates them.
-- NOTE: reconcile this script against the actual deployed procedure - table/column
-- names here are assumed to match the C# repository's expectations, not confirmed.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.uspEvidenceGetFieldsByTypeId
    @metricEvidenceTypeId BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ef.EvidenceFieldId,
        ef.EvidenceKey
    FROM dbo.EvidenceField ef
    WHERE ef.MetricEvidenceTypeId = @metricEvidenceTypeId;
END
GO
