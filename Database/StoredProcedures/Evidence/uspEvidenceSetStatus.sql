-- =============================================================================
-- Stored Procedure : dbo.uspEvidenceSetStatus
-- Type             : CUD (update)
-- Description      : Directly sets Evidence.Status to a raw StatusId (no
--                    StatusCode->MasterStatus resolution - unlike
--                    uspEvidenceUpdateProcessingResult). Used by the /process
--                    endpoint to bracket a request: set to Processing (19) as soon
--                    as the request is accepted, and to Failed (21) if any error
--                    occurs anywhere in the pipeline. Does not touch isReviewRequired.
-- Errors           : 51001 - EvidenceId not found (custom error, translated by the
--                    repository to NotFoundException).
-- NOTE: reconcile this script against the actual deployed procedure - not yet confirmed live.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.uspEvidenceSetStatus
    @EvidenceId BIGINT,
    @StatusId   INT,
    @UpdatedBy  BIGINT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        UPDATE dbo.Evidence
        SET
            Status       = @StatusId,
            UpdatedBy    = @UpdatedBy,
            UpdatedOnUTC = SYSUTCDATETIME()
        WHERE EvidenceId = @EvidenceId;

        IF @@ROWCOUNT = 0
        BEGIN
            THROW 51001, 'uspEvidenceSetStatus: EvidenceId not found.', 1;
        END
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO
