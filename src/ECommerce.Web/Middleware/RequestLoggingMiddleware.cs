using ECommerce.Common.Constants;
using ECommerce.Common.Helpers;
using System.Diagnostics;

namespace ECommerce.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[AppConstants.Headers.CorrelationId].FirstOrDefault()
                            ?? HashHelper.GenerateCorrelationId();

        context.Items[AppConstants.Headers.CorrelationId] = correlationId;
        context.Response.Headers[AppConstants.Headers.CorrelationId] = correlationId;

        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "HTTP {Method} {Path} started | CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            correlationId);

        await _next(context);

        sw.Stop();

        _logger.LogInformation(
            "HTTP {Method} {Path} completed with {StatusCode} in {Elapsed}ms | CorrelationId: {CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds,
            correlationId);
    }
}
