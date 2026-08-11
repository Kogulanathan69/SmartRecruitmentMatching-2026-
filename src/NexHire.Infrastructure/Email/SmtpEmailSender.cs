using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using NexHire.Application.Interfaces.Services;

namespace NexHire.Infrastructure.Email;

public sealed class SmtpEmailSender(IConfiguration configuration) : IEmailSender
{
    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var host = configuration["Email:SmtpHost"]?.Trim();
        if (string.IsNullOrWhiteSpace(host)) host = "smtp.gmail.com";

        var port = int.TryParse(configuration["Email:SmtpPort"], out var configuredPort)
            ? configuredPort
            : 587;

        var username = configuration["Email:Username"]?.Trim();
        var password = configuration["Email:Password"];
        var fromEmail = configuration["Email:FromEmail"]?.Trim();
        var fromName = configuration["Email:FromName"]?.Trim();
        var enableSsl = !bool.TryParse(configuration["Email:EnableSsl"], out var configuredSsl) || configuredSsl;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("Email SMTP credentials are not configured.");

        if (string.IsNullOrWhiteSpace(fromEmail)) fromEmail = username;
        if (string.IsNullOrWhiteSpace(fromName)) fromName = "NexHire";

        cancellationToken.ThrowIfCancellationRequested();

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 15000
        };

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }
}