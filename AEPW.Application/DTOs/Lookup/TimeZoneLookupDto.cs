namespace AEPW.Application.DTOs.Lookup;

public sealed class TimeZoneLookupDto
{
    public long TimeZoneId { get; set; }

    public string TimeZoneCode { get; set; } = string.Empty;

    public string TimeZoneName { get; set; } = string.Empty;
}