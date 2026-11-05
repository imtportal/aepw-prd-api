namespace AEPW.Domain.Models.Lookup;

public sealed class MetricUnitLookupModel
{
    public string MetricUnitId { get; set; } = default!;
    public string UnitCode { get; set; } = default!;
    public string UnitName { get; set; } = default!;
}
