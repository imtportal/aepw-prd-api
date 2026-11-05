namespace AEPW.Domain.Models.Lookup;

public sealed class StatusLookupModel
{
    public string StatusId { get; set; } = default!;
    public string StatusCode { get; set; } = default!;
    public string StatusName { get; set; } = default!;
    public int DisplayOrder { get; set; }
    public bool IsDefault { get; set; }
}
