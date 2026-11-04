using Microsoft.Extensions.Options;
using SimplifiedPayments.Application.Abstractions;

namespace SimplifiedPayments.Infrastructure.ExternalServices;

public sealed class HttpNotificationService(
    HttpClient http, IOptions<ExternalServicesOptions> options
    ) : INotificationService
{
    public async Task NotifyAsync(string email, string message, CancellationToken ct = default)
    {
        using var response = await 
            http.PostAsJsonAsync(options.Value.NotifierUrl, new { email, message }, ct);
        response.EnsureSuccessStatusCode();
    }
}
