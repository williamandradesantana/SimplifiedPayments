using Microsoft.Extensions.Options;
using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Application.Exceptions;
using SimplifiedPayments.Domain.Entities;
using System.Net;

namespace SimplifiedPayments.Infrastructure.ExternalServices;

public sealed class HttpTransferAuthorizer(
    HttpClient http,
    IOptions<ExternalServicesOptions> options,
    ILogger<HttpTransferAuthorizer> logger
) : ITransferAuthorizer
{
    public async Task<bool> IsAuthorizedAsync(User payer, User payee, decimal amount, CancellationToken ct = default)
    {
        try
        {
            using var response = await http.GetAsync(options.Value.AuthorizerUrl, ct);
            if (response.StatusCode == HttpStatusCode.Forbidden) return false;

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<AuthorizerResponse>(ct);
            return body?.Data?.Authorization == true;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Authorizer call failed");
            throw new ExternalServiceUnavailableException("Authorizer service is unavailable", ex);
        }
    }

    private sealed record AuthorizerResponse(AuthorizerData? Data);
    private sealed record AuthorizerData(bool Authorization);
}
