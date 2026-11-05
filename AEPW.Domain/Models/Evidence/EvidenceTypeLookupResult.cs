namespace AEPW.Domain.Models.Evidence;

/// <summary>
/// Result of resolving a dbo.EvidenceType row by its Code (uspEvidenceGetTypeByCode). Not
/// metric-scoped - EvidenceType.Code is the sole lookup key.
/// </summary>
public sealed class EvidenceTypeLookupResult
{
    public long EvidenceTypeId { get; set; }

    /// <summary>
    /// Explicit, data-driven flag: only evidence types with this set to true get FX
    /// (Frankfurter) conversion attempted. Defaults to false when absent/DBNull.
    /// </summary>
    public bool IsExchangeRateRequired { get; set; }
}
