using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using NexHire.Application.Interfaces.Services;

namespace NexHire.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:SmtpHost"];
        var portText = _configuration["Email:SmtpPort"];
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        var fromEmail = _configuration["Email:FromEmail"];
        var fromName = _configuration["Email:FromName"];

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(portText) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException(
                "Email SMTP configuration is missing.");
        }

        if (!int.TryParse(portText, out var port))
        {
            throw new InvalidOperationException(
                "Email SMTP port is invalid.");
        }

        using var smtpClient = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password)
        };

        using var mailMessage = new MailMessage
        {
            From = new MailAddress(
                fromEmail,
                string.IsNullOrWhiteSpace(fromName)
                    ? "NexHire"
                    : fromName),

            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        cancellationToken.ThrowIfCancellationRequested();

        await smtpClient.SendMailAsync(mailMessage);
    }
}