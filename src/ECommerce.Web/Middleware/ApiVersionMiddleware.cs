using ECommerce.Common.Constants;

namespace ECommerce.Web.Middleware;

public class ApiVersionMiddleware
{
    private readonly RequestDelegate _next;
    private const string CurrentVersion = "1.0";

    public ApiVersionMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers[AppConstants.Headers.ApiVersion] = CurrentVersion;
        await _next(context);
    }
}
