using System.Net.Http.Json;
using RentADad.Application.Abstractions.Notifications;

namespace RentADad.Api.Notifications;

public sealed class WebhookNotificationSender : INotificationSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebhookNotificationSender> _logger;

    public WebhookNotificationSender(HttpClient httpClient, IConfiguration configuration, ILogger<WebhookNotificationSender> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task NotifyAsync(string eventName, object payload, CancellationToken cancellationToken = default)
    {
        var endpoint = _configuration["Notifications:WebhookUrl"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            return;
        }

        var request = new
        {
            eventName,
            payload,
            occurredUtc = DateTime.UtcNow
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Notification webhook returned {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Notification webhook failed");
        }
    }
}
