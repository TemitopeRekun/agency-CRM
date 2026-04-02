using Crm.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;

namespace Crm.Infrastructure.Services;

public class SlackService : ISlackService
{
    private readonly ILogger<SlackService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public SlackService(ILogger<SlackService> logger, IConfiguration configuration, HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task SendNotificationAsync(string message)
    {
        var webhookUrl = _configuration["Notifications:Slack:WebhookUrl"];
        
        if (string.IsNullOrEmpty(webhookUrl))
        {
            _logger.LogWarning("SLACK_LOG_ONLY: {Message}", message);
            return;
        }

        try
        {
            var payload = new { text = message };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(webhookUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to send Slack notification. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack notification");
        }
    }
}
