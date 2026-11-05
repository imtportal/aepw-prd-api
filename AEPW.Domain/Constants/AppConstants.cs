namespace AEPW.Domain.Constants;

public static class AppConstants
{
    public const string DefaultCorsPolicyName = "AEPWCorsPolicy";
    public const string ApiTitle              = "AEPW API";
    public const string ApiVersionV1          = "v1";
    public const int    DefaultPageSize       = 20;
    public const int    MaxPageSize           = 100;
}

/// <summary>
/// EVIDENCE-module StatusCode values from MasterStatus, passed to
/// uspEvidenceUpdateProcessingResult (which resolves them to the int StatusId
/// stored on Evidence.Status).
/// </summary>
public static class EvidenceStatuses
{
    /// <summary>Average extraction confidence met the threshold - accepted.</summary>
    public const string Validated = "VALIDATED";

    /// <summary>Average extraction confidence was below the threshold - needs manual review.</summary>
    public const string NeedsReview = "NEEDSREVIEW";
}

/// <summary>
/// Raw Evidence.Status ids (distinct from the StatusCode-driven values in
/// <see cref="EvidenceStatuses"/>) used to bracket a /process request: set to
/// Processing as soon as the request is accepted, and to Failed if any error
/// occurs anywhere in the pipeline.
/// </summary>
public static class EvidenceStatusIds
{
    public const int Processing = 19;
    public const int Failed = 21;
}
