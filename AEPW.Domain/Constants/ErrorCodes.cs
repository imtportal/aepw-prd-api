namespace AEPW.Domain.Constants;

public static class ErrorCodes
{
    public const string NotFound           = "NOT_FOUND";
    public const string ValidationFailed   = "VALIDATION_FAILED";
    public const string Conflict           = "CONFLICT";
    public const string Unauthorized       = "UNAUTHORIZED";
    public const string Forbidden          = "FORBIDDEN";
    public const string InternalError      = "INTERNAL_ERROR";
    public const string DatabaseError      = "DATABASE_ERROR";
    public const string ExternalService    = "EXTERNAL_SERVICE_ERROR";

    /// <summary>Evidence processing failed and will NEVER succeed on retry (HTTP 422). Do not redeliver.</summary>
    public const string PermanentProcessingFailure = "PERMANENT_PROCESSING_FAILURE";

    /// <summary>Evidence processing failed but MAY succeed on retry (HTTP 503). Safe to redeliver.</summary>
    public const string TransientProcessingFailure = "TRANSIENT_PROCESSING_FAILURE";
}
