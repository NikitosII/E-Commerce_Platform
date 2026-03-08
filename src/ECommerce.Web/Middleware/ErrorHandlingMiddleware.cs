using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace ECommerce.Web.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private static Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
            InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Unauthorized."),
            DbUpdateException dbEx => (HttpStatusCode.BadRequest, ExtractDbMessage(dbEx)),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = JsonSerializer.Serialize(new
        {
            status = (int)statusCode,
            error = message,
            timestamp = DateTime.UtcNow
        });

        return context.Response.WriteAsync(payload);
    }

    private static string ExtractDbMessage(DbUpdateException ex)
    {
        var inner = ex.InnerException?.Message ?? ex.Message;
        // FK violation
        if (inner.Contains("foreign key") || inner.Contains("violates foreign key"))
            return "A referenced record does not exist (foreign key violation).";
        // Unique constraint
        if (inner.Contains("unique") || inner.Contains("duplicate"))
            return "A record with the same unique value already exists.";
        return "A database error occurred while saving changes.";
    }
}
