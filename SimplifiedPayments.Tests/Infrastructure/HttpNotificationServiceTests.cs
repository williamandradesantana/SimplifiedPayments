using FluentAssertions;
using Microsoft.Extensions.Options;
using SimplifiedPayments.Infrastructure.ExternalServices;
using SimplifiedPayments.Tests.Fakes;
using System.Net;

namespace SimplifiedPayments.Tests.Infrastructure;

public class HttpNotificationServiceTests
{
    private const string Url = "https://notify.test/api/v1/notify";

    private static (HttpNotificationService Sut, StubHttpMessageHandler Handler) Create(HttpStatusCode status)
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(status));
        var options = Options.Create(new ExternalServicesOptions { AuthorizerUrl = "https://auth.test/", NotifierUrl = Url });
        return (new HttpNotificationService(new HttpClient(handler), options), handler);
    }

    [Fact]
    public async Task Notify_PostsEmailAndMessageToConfiguredUrl()
    {
        var (sut, handler) = Create(HttpStatusCode.NoContent);

        await sut.NotifyAsync("payee@example.com", "You received 30.00.");

        var call = handler.Calls.Should().ContainSingle().Subject;
        call.Method.Should().Be(HttpMethod.Post);
        call.Url.Should().Be(Url);
        call.Body.Should().Contain("payee@example.com").And.Contain("You received 30.00.");
    }

    [Fact]
    public async Task Notify_ServiceUnavailable_Throws()
    {
        var (sut, _) = Create(HttpStatusCode.GatewayTimeout);

        Func<Task> act = () => sut.NotifyAsync("payee@example.com", "hello");

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}