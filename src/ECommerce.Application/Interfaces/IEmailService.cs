namespace ECommerce.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default);
    Task SendOrderConfirmationAsync(string to, Guid orderId, CancellationToken cancellationToken = default);
    Task SendOrderShippedAsync(string to, Guid orderId, string trackingNumber, CancellationToken cancellationToken = default);
}
