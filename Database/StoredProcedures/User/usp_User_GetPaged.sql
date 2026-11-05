-- =============================================================================
-- Stored Procedure : usp_User_GetPaged
-- Type             : READ
-- Description      : Server-side paging with optional search and dynamic sort
--                    Total row count returned via OUTPUT parameter — no extra
--                    round-trip required.
-- =============================================================================
CREATE OR ALTER PROCEDURE dbo.usp_User_GetPaged
	@PageNumber     INT             = 1,
	@PageSize       INT             = 20,
	@SearchTerm     NVARCHAR(200)   = NULL,
	@SortBy         NVARCHAR(50)    = NULL,     -- 'FirstName'|'LastName'|'Email'|'CreatedAt'
	@SortDirection  TINYINT         = 0,        -- 0=ASC  1=DESC
	@TotalCount     INT             OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

	-- ── Total count (before paging) ─────────────────────────────────────────
	SELECT @TotalCount = COUNT(*)
	FROM   dbo.Users
	WHERE  IsDeleted = 0
	  AND  (
				@SearchTerm IS NULL
			 OR FirstName   LIKE '%' + @SearchTerm + '%'
			 OR LastName    LIKE '%' + @SearchTerm + '%'
			 OR Email       LIKE '%' + @SearchTerm + '%'
		   );

	-- ── Paged result set ─────────────────────────────────────────────────────
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
	  AND (
			  @SearchTerm IS NULL
		   OR u.FirstName LIKE '%' + @SearchTerm + '%'
		   OR u.LastName  LIKE '%' + @SearchTerm + '%'
		   OR u.Email     LIKE '%' + @SearchTerm + '%'
		  )
	ORDER BY
		CASE WHEN @SortBy = 'FirstName'  AND @SortDirection = 0 THEN u.FirstName  END ASC,
		CASE WHEN @SortBy = 'FirstName'  AND @SortDirection = 1 THEN u.FirstName  END DESC,
		CASE WHEN @SortBy = 'Email'      AND @SortDirection = 0 THEN u.Email      END ASC,
		CASE WHEN @SortBy = 'Email'      AND @SortDirection = 1 THEN u.Email      END DESC,
		CASE WHEN @SortBy = 'CreatedAt'  AND @SortDirection = 1 THEN u.CreatedAt  END DESC,
		CASE WHEN @SortBy = 'CreatedAt'  AND @SortDirection = 0 THEN u.CreatedAt  END ASC,
		-- default: LastName ASC
		CASE WHEN @SortBy IS NULL        OR @SortBy = 'LastName' THEN u.LastName  END ASC
	OFFSET @Offset ROWS
	FETCH  NEXT @PageSize ROWS ONLY;
END
GO
