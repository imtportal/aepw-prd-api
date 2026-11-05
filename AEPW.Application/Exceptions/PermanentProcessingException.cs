namespace AEPW.Application.Exceptions;

/// <summary>
/// Evidence processing failed in a way that will NEVER succeed on retry with the same input:
/// agent refusal/non-JSON output, malformed or schema-invalid agent JSON, or a corrupt/unreadable
/// input file. Maps to HTTP 422 - the Worker must NOT redeliver the Service Bus message for this
/// EvidenceId, and the evidence is marked Failed.
/// </summary>
public sealed class PermanentProcessingException : Exception
{
    public PermanentProcessingException(string message) : base(message) { }
    public PermanentProcessingException(string message, Exception innerException) : base(message, innerException) { }
}
