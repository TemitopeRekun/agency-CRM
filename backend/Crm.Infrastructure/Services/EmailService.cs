using Crm.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Crm.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;

    public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        // PLACEHOLDER: Logging implementation
        // REAL PLAN: In production, we will use MailKit and SmtpSettings from Configuration.
        
        _logger.LogInformation("--- EMAIL NOTIFICATION (PLACEHOLDER) ---");
        _logger.LogInformation("To: {To}", to);
        _logger.LogInformation("Subject: {Subject}", subject);
        _logger.LogInformation("Body: {Body}", body);
        _logger.LogInformation("----------------------------------------");

        await Task.CompletedTask;
    }
}
