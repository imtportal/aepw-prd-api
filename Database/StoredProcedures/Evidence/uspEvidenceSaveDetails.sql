-- =============================================================================
-- Stored Procedure : dbo.uspEvidenceSaveDetails
-- Type             : CUD (delete + insert)
-- Description      : Replaces all EvidenceDetail rows for @EvidenceId with the rows
--                    in @DetailsJson. Idempotent on reprocess.
-- @CreatedBy       : service-account user id stamped on each new EvidenceDetail row.
-- Errors           : 51002 - malformed @DetailsJson.
-- NOTE: reconcile this script against the actual deployed procedure - it had fallen out of
-- sync with the C# payload (LineItemValue/IsLineItem/PageNumber were already being sent but
-- not read here). This revision adds those plus the new IsUnique/OriginalPage columns
-- (hybrid page-duplicate detection) - confirm EvidenceDetail has all of these columns before
-- deploying.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.uspEvidenceSaveDetails
    @EvidenceId  BIGINT,
    @DetailsJson NVARCHAR(MAX),
    @CreatedBy   BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @DetailsJson IS NOT NULL AND ISJSON(@DetailsJson) = 0
    BEGIN
        THROW 51002, 'uspEvidenceSaveDetails: @DetailsJson is not valid JSON.', 1;
    END;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.EvidenceDetail
        WHERE EvidenceId = @EvidenceId;

        INSERT INTO dbo.EvidenceDetail
            (EvidenceId, EvidenceFieldId, Value, LineItemValue, IsLineItem, PageNumber,
             IsUnique, OriginalPage, ConfidenceScore, CreatedBy, CreatedOnUTC)
        SELECT
            @EvidenceId,
            j.EvidenceFieldId,
            j.Value,
            j.LineItemValue,
            j.IsLineItem,
            j.PageNumber,
            j.IsUnique,
            j.OriginalPage,
            j.ConfidenceScore,
            @CreatedBy,
            SYSUTCDATETIME()
        FROM OPENJSON(@DetailsJson)
        WITH (
            EvidenceFieldId  BIGINT        '$.EvidenceFieldId',
            Value            NVARCHAR(MAX) '$.Value',
            LineItemValue    NVARCHAR(MAX) '$.LineItemValue',
            IsLineItem       BIT           '$.IsLineItem',
            PageNumber       INT           '$.PageNumber',
            IsUnique         BIT           '$.IsUnique',
            OriginalPage     INT           '$.OriginalPage',
            ConfidenceScore  FLOAT         '$.ConfidenceScore'
        ) j;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        THROW;
    END CATCH
END
GO