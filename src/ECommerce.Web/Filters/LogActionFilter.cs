using ECommerce.Common.Constants;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.Web.Filters;

public class LogActionFilter : IActionFilter
{
    private readonly ILogger<LogActionFilter> _logger;

    public LogActionFilter(ILogger<LogActionFilter> logger) => _logger = logger;

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var correlationId = context.HttpContext.Items[AppConstants.Headers.CorrelationId]?.ToString();
        _logger.LogInformation(
            "Action executing: {Controller}.{Action} | CorrelationId: {CorrelationId}",
            context.RouteData.Values["controller"],
            context.RouteData.Values["action"],
            correlationId);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception is not null)
        {
            _logger.LogWarning(
                "Action {Controller}.{Action} threw: {Exception}",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"],
                context.Exception.Message);
        }
    }
}
