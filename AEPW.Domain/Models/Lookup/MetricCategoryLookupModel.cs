namespace AEPW.Domain.Models.Lookup;

public sealed class MetricCategoryLookupModel
{
    public string CategoryId { get; set; } = default!;
    public string CategoryCode { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Color { get; set; }
}
