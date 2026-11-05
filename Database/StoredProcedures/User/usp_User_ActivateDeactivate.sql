-- =============================================================================
-- Stored Procedure : usp_User_Activate
-- Type             : CUD  (Status change)
-- Description      : Sets Status = 1 (Active) for a non-deleted user.
--                    RULE: CUD SPs never SELECT / return fetch data.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_User_Activate
	@Id         INT,
	@UpdatedBy  INT
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @Id AND IsDeleted = 0)
	BEGIN
		RAISERROR('User not found or has been deleted.', 16, 1);
		RETURN;
	END

	UPDATE dbo.Users
	SET
		Status    = 1,              -- UserStatus.Active
		UpdatedAt = SYSUTCDATETIME(),
		UpdatedBy = @UpdatedBy
	WHERE Id        = @Id
	  AND IsDeleted = 0;
END
GO

-- =============================================================================
-- Stored Procedure : usp_User_Deactivate
-- Type             : CUD  (Status change)
-- Description      : Sets Status = 2 (Inactive) for a non-deleted user.
--                    RULE: CUD SPs never SELECT / return fetch data.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_User_Deactivate
	@Id         INT,
	@UpdatedBy  INT
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @Id AND IsDeleted = 0)
	BEGIN
		RAISERROR('User not found or has been deleted.', 16, 1);
		RETURN;
	END

	UPDATE dbo.Users
	SET
		Status    = 2,              -- UserStatus.Inactive
		UpdatedAt = SYSUTCDATETIME(),
		UpdatedBy = @UpdatedBy
	WHERE Id        = @Id
	  AND IsDeleted = 0;
END
GO
