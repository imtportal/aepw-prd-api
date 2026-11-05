-- =============================================================================
-- Stored Procedure : usp_User_SoftDelete
-- Type             : CUD  (Delete — soft)
-- Description      : Marks a user as deleted; row is never physically removed.
--                    RULE: CUD SPs never SELECT / return fetch data.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_User_SoftDelete
	@Id         INT,
	@DeletedBy  INT
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @Id AND IsDeleted = 0)
	BEGIN
		RAISERROR('User not found or already deleted.', 16, 1);
		RETURN;
	END

	UPDATE dbo.Users
	SET
		IsDeleted = 1,
		UpdatedAt = SYSUTCDATETIME(),
		UpdatedBy = @DeletedBy
	WHERE Id = @Id;
END
GO
