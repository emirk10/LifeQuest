using System.Net;
using System.Net.Mail;
using LifeQuestAPI.Application.Abstractions.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LifeQuestAPI.Infrastructure.Services.Email;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
    {
        try
        {
            var host = _configuration["EmailSettings:Host"] ?? "localhost";
            var portString = _configuration["EmailSettings:Port"] ?? "1025";
            var port = int.Parse(portString);
            var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@lifequest.com";
            var fromName = _configuration["EmailSettings:FromName"] ?? "LifeQuest";

            using var client = new SmtpClient(host, port);
            
            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {toEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {toEmail}", toEmail);
            // We shouldn't throw an exception to interrupt the registration flow
            // in a real app, maybe enqueue this or handle gracefully.
        }
    }
}
