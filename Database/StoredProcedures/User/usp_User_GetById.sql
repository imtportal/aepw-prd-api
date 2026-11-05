-- =============================================================================
-- Stored Procedure : usp_User_GetById
-- Type             : READ
-- Description      : Fetches a single active (non-deleted) user by primary key
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_User_GetById
	@Id INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		u.Id,
		u.FirstName,
		u.LastName,
		u.Email,
		u.PhoneNumber,
		u.Status,
		u.IsDeleted,
		u.CreatedAt,
		u.CreatedBy,
		u.UpdatedAt,
		u.UpdatedBy
	FROM dbo.Users u
	WHERE u.Id        = @Id
	  AND u.IsDeleted = 0;
END
GO
