using ECommerce.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ECommerce.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "localhost";
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:Username"] ?? string.Empty;
        var smtpPass = _configuration["Email:Password"] ?? string.Empty;
        var fromAddress = _configuration["Email:FromAddress"] ?? "noreply@ecommerce.local";

        using var client = new SmtpClient(smtpHost, smtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(smtpUser, smtpPass)
        };

        using var message = new MailMessage(fromAddress, to, subject, body)
        {
            IsBodyHtml = isHtml
        };

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Email sent to {To} with subject '{Subject}'", to, subject);
    }

    public Task SendOrderConfirmationAsync(string to, Guid orderId, CancellationToken cancellationToken = default) =>
        SendAsync(to, "Order Confirmation",
            $"<h2>Thank you for your order!</h2><p>Your order ID: <strong>{orderId}</strong></p>",
            cancellationToken: cancellationToken);

    public Task SendOrderShippedAsync(string to, Guid orderId, string trackingNumber, CancellationToken cancellationToken = default) =>
        SendAsync(to, "Your Order Has Shipped",
            $"<h2>Your order is on its way!</h2><p>Order ID: <strong>{orderId}</strong><br/>Tracking: <strong>{trackingNumber}</strong></p>",
            cancellationToken: cancellationToken);
}
