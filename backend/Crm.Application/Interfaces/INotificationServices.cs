namespace Crm.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public interface ISlackService
{
    Task SendNotificationAsync(string message);
}
