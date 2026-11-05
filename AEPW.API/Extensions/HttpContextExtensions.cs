using AEPW.API.Constants;

namespace AEPW.API.Extensions;

public static class HttpContextExtensions
{
    public static long? GetCurrentUserId(this HttpContext httpContext)
    {
        if (!httpContext.Request.Headers.TryGetValue(RequestHeaders.UserId, out var value))
        {
            return null;
        }

        return long.TryParse(value, out var userId)
            ? userId
            : null;
    }
}