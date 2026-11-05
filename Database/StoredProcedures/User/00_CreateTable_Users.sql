-- =============================================================================
-- Table: Users
-- Soft-delete pattern: IsDeleted flag, never physically remove rows
-- Audit: CreatedBy/UpdatedBy store user IDs only (no name denormalization)
-- =============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Users' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
	CREATE TABLE dbo.Users
	(
		Id          INT             NOT NULL IDENTITY(1,1),
		FirstName   NVARCHAR(100)   NOT NULL,
		LastName    NVARCHAR(100)   NOT NULL,
		Email       NVARCHAR(256)   NOT NULL,
		PhoneNumber NVARCHAR(20)    NOT NULL DEFAULT '',
		Status      TINYINT         NOT NULL DEFAULT 1,   -- 1=Active 2=Inactive 3=Suspended
		IsDeleted   BIT             NOT NULL DEFAULT 0,
		CreatedAt   DATETIME2(7)    NOT NULL DEFAULT SYSUTCDATETIME(),
		CreatedBy   INT             NOT NULL,
		UpdatedAt   DATETIME2(7)    NULL,
		UpdatedBy   INT             NULL,

		CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
		CONSTRAINT UQ_Users_Email UNIQUE (Email)
	);

	CREATE NONCLUSTERED INDEX IX_Users_Email
		ON dbo.Users (Email)
		WHERE IsDeleted = 0;

	CREATE NONCLUSTERED INDEX IX_Users_Status_IsDeleted
		ON dbo.Users (Status, IsDeleted)
		INCLUDE (FirstName, LastName, Email);
END
GO
