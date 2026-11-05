-- =============================================================================
-- Stored Procedure : dbo.uspEvidenceUpdateProcessingResult
-- Type             : CUD (update)
-- Description      : Sets the Evidence row's review flag, status, evidenced value,
--                    validation result, and IsWithinReporting flag once extraction
--                    has completed. Combines the confidence-flag update and the
--                    status transition in a single write. Evidence.Status is an INT
--                    FK to MasterStatus.StatusId, so @StatusCode (a string, e.g.
--                    'VALIDATED' / 'NEEDSREVIEW') is resolved to its StatusId for
--                    Module = 'EVIDENCE' before the update. @EvidencedValue and
--                    @ValidationResult are optional (NULL if the agent output had
--                    no "evidenced"/"validation" section). @IsWithinReporting
--                    defaults to 1 (matches the column default) - the repository
--                    always passes an explicit mapped value, but the default keeps
--                    this SP safe for any other caller.
-- Errors           : 51001 - EvidenceId not found.
--                    51003 - @StatusCode not found in MasterStatus for Module 'EVIDENCE'.
--                    (Both custom errors, translated by the repository to
--                    NotFoundException / ValidationException respectively.)
-- NOTE: reconcile this script against the actual deployed procedure - MasterStatus's
-- column names here (StatusId/Module/StatusCode) are assumed, not confirmed.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.uspEvidenceUpdateProcessingResult
    @EvidenceId        BIGINT,
    @IsReviewRequired  BIT,
    @StatusCode        NVARCHAR(50),
    @UpdatedBy         BIGINT,
    @EvidencedValue    NVARCHAR(MAX) = NULL,
    @ValidationResult  NVARCHAR(MAX) = NULL,
    @IsWithinReporting BIT           = 1
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        DECLARE @StatusId INT;

        SELECT @StatusId = StatusId
        FROM dbo.MasterStatus
        WHERE Module     = 'EVIDENCE'
          AND StatusCode = @StatusCode;

        IF @StatusId IS NULL
        BEGIN
            THROW 51003, 'uspEvidenceUpdateProcessingResult: Unknown @StatusCode for Module EVIDENCE.', 1;
        END

        UPDATE dbo.Evidence
        SET
            isReviewRequired  = @IsReviewRequired,
            Status            = @StatusId,
            EvidencedValue    = @EvidencedValue,
            ValidationResult  = @ValidationResult,
            IsWithinReporting = @IsWithinReporting,
            UpdatedBy         = @UpdatedBy,
            UpdatedOnUTC      = SYSUTCDATETIME()
        WHERE EvidenceId = @EvidenceId;

        IF @@ROWCOUNT = 0
        BEGIN
            THROW 51001, 'uspEvidenceUpdateProcessingResult: EvidenceId not found.', 1;
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
