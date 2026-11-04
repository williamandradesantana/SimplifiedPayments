namespace SimplifiedPayments.Tests.Fakes;

internal sealed class StubHttpMessageHandler (
    Func<HttpRequestMessage, HttpResponseMessage> responder
    ) : HttpMessageHandler
{
    public List<(HttpMethod Method, string? Url, string? Body)> Calls { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct
        )
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(ct);
        Calls.Add((request.Method, request.RequestUri?.ToString(), body));

        return responder(request);
    }
}

