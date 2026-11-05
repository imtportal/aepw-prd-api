/// <summary>
/// Paged result wrapper carrying metadata alongside the data items.
/// </summary>
namespace AEPW.Application.Contracts.Responses;

public sealed class PagedResponse<T>
{
    public IReadOnlyList<T> Items       { get; init; } = [];
    public int              TotalRecords  { get; init; }
    public int              PageNumber  { get; init; }
    public int              PageSize    { get; init; }
    public int              TotalPages  => (int)Math.Ceiling((double)TotalRecords / PageSize);
    public bool             HasPrevious => PageNumber > 1;
    public bool             HasNext     => PageNumber < TotalPages;
}
