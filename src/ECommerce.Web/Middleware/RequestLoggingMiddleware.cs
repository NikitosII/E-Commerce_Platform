using ECommerce.Common.Constants;
using ECommerce.Common.Extensions;
using ECommerce.Common.Helpers;
using System.Diagnostics;

namespace ECommerce.Web.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private const int MaxPathLength = 100;

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

        var path = context.Request.Path.ToString().Truncate(MaxPathLength);
        var sw = Stopwatch.StartNew();

        _logger.LogInformation(
            "HTTP {Method} {Path} started | CorrelationId: {CorrelationId}",
            context.Request.Method,
            path,
            correlationId);

        await _next(context);

        sw.Stop();

        _logger.LogInformation(
            "HTTP {Method} {Path} completed with {StatusCode} in {Elapsed}ms | CorrelationId: {CorrelationId}",
            context.Request.Method,
            path,
            context.Response.StatusCode,
            sw.ElapsedMilliseconds,
            correlationId);
    }
}
