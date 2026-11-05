-- =============================================================================
-- Stored Procedure : usp_User_GetAll
-- Type             : READ
-- Description      : Returns all active (non-deleted) users ordered by LastName
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_User_GetAll
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
	WHERE u.IsDeleted = 0
	ORDER BY u.LastName, u.FirstName;
END
GO
