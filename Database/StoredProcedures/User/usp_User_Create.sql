-- =============================================================================
-- Stored Procedure : usp_User_Create
-- Type             : CUD  (Create)
-- Description      : Inserts a new user row.
--                    RULE: CUD SPs never SELECT / return fetch data.
--                    New PK is returned via OUTPUT parameter only.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_User_Create
	@FirstName      NVARCHAR(100),
	@LastName       NVARCHAR(100),
	@Email          NVARCHAR(256),
	@PhoneNumber    NVARCHAR(20)  = '',
	@CreatedBy      INT,
	@NewId          INT           OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	-- Guard: duplicate active email
	IF EXISTS (
		SELECT 1 FROM dbo.Users
		WHERE  Email     = @Email
		  AND  IsDeleted = 0
	)
	BEGIN
		RAISERROR('A user with this email address already exists.', 16, 1);
		RETURN;
	END

	INSERT INTO dbo.Users
		(FirstName, LastName, Email, PhoneNumber, Status, IsDeleted, CreatedAt, CreatedBy)
	VALUES
		(@FirstName, @LastName, @Email, @PhoneNumber, 1, 0, SYSUTCDATETIME(), @CreatedBy);

	SET @NewId = SCOPE_IDENTITY();
END
GO
