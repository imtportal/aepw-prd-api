namespace AEPW.Domain.Models.Lookup;

public sealed class ParentMetricLookupModel
{
    public string MetricId { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string CategoryId { get; set; } = default!;
}
