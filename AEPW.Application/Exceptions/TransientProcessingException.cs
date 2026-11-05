namespace AEPW.Application.Exceptions;

/// <summary>
/// Evidence processing failed in a way that MAY succeed if retried later: SDK network timeout,
/// a transient HTTP status (408/429/500/502/503/504) from the agent endpoint, or a transient SQL
/// error (deadlock, command timeout, Azure SQL throttling). Maps to HTTP 503 - the Worker SHOULD
/// redeliver the Service Bus message for this EvidenceId. The evidence is deliberately left as-is
/// (NOT marked Failed) so a subsequent retry can still complete successfully.
/// </summary>
public sealed class TransientProcessingException : Exception
{
    public TransientProcessingException(string message) : base(message) { }
    public TransientProcessingException(string message, Exception innerException) : base(message, innerException) { }
}
