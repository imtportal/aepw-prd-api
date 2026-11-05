CREATE OR ALTER PROCEDURE dbo.uspEmailNotificationTemplateSave
    @EmailNotificationTemplateId BIGINT,
    @TemplateCode VARCHAR(50),
    @TemplateName VARCHAR(100),
    @SubjectTemplate NVARCHAR(500),
    @BodyTemplate NVARCHAR(MAX),
    @RecipientTo NVARCHAR(500),
    @RecipientCc NVARCHAR(500) = NULL,
    @IsActive BIT,
    @CurrentUserId BIGINT,
    @SavedEmailNotificationTemplateId BIGINT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @SavedEmailNotificationTemplateId = NULL;

    IF @EmailNotificationTemplateId > 0
    BEGIN
        UPDATE dbo.EmailNotificationTemplate
        SET
            TemplateCode = @TemplateCode,
            TemplateName = @TemplateName,
            SubjectTemplate = @SubjectTemplate,
            BodyTemplate = @BodyTemplate,
            RecipientTo = @RecipientTo,
            RecipientCc = @RecipientCc,
            IsActive = @IsActive,
            UpdatedBy = @CurrentUserId,
            UpdatedOnUTC = SYSUTCDATETIME()
        WHERE EmailNotificationTemplateId = @EmailNotificationTemplateId;

        IF @@ROWCOUNT = 0
        BEGIN
            RETURN;
        END;

        SET @SavedEmailNotificationTemplateId = @EmailNotificationTemplateId;
        RETURN;
    END;

    INSERT INTO dbo.EmailNotificationTemplate
    (
        TemplateCode,
        TemplateName,
        SubjectTemplate,
        BodyTemplate,
        RecipientTo,
        RecipientCc,
        IsActive,
        CreatedBy
    )
    VALUES
    (
        @TemplateCode,
        @TemplateName,
        @SubjectTemplate,
        @BodyTemplate,
        @RecipientTo,
        @RecipientCc,
        @IsActive,
        @CurrentUserId
    );

    SET @SavedEmailNotificationTemplateId =
        CONVERT(BIGINT, SCOPE_IDENTITY());
END;
GO
