/// <summary>
/// Unified API response envelope used by every endpoint.
/// </summary>
namespace AEPW.Application.Contracts.Responses;

public sealed class ApiResponse<T>
{
    public bool     Success    { get; init; }
    public string   Message    { get; init; } = string.Empty;
    public T?       Data       { get; init; }
    public string?  ErrorCode  { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, string? errorCode = null,
        IReadOnlyList<string>? errors = null) =>
        new() { 
            Success = false, 
            Message = message, 
            ErrorCode = errorCode,
            Errors = errors ?? [] };
}
