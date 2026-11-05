-- =============================================================================
-- Stored Procedure : usp_User_Update
-- Type             : CUD  (Update)
-- Description      : Updates mutable fields of an existing active user.
--                    RULE: CUD SPs never SELECT / return fetch data.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_User_Update
	@Id             INT,
	@FirstName      NVARCHAR(100),
	@LastName       NVARCHAR(100),
	@PhoneNumber    NVARCHAR(20),
	@UpdatedBy      INT
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
		FirstName   = @FirstName,
		LastName    = @LastName,
		PhoneNumber = @PhoneNumber,
		UpdatedAt   = SYSUTCDATETIME(),
		UpdatedBy   = @UpdatedBy
	WHERE Id        = @Id
	  AND IsDeleted = 0;
END
GO
