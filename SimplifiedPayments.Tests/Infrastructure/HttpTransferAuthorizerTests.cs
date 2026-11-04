using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SimplifiedPayments.Application.Exceptions;
using SimplifiedPayments.Domain.Entities;
using SimplifiedPayments.Infrastructure.ExternalServices;
using SimplifiedPayments.Tests.Fakes;
using SimplifiedPayments.Tests.Helpers;
using System.Net;
using System.Text;

namespace SimplifiedPayments.Tests.Infrastructure;

public class HttpTransferAuthorizerTests
{
    private const string Url = "https://auth.rest/api/v2/authorize";

    private static readonly User Payer = UserFactory.Common("a@example.com").WithId(1);
    private static readonly User Payee = UserFactory.Common("b@example.com").WithId(2);

    private static (HttpTransferAuthorizer Sut, StubHttpMessageHandler Handler) Create(
       Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new StubHttpMessageHandler(responder);
        var options = Options.Create(new ExternalServicesOptions { AuthorizerUrl = Url, NotifierUrl = "https://notify.test/" });
        var sut = new HttpTransferAuthorizer(new HttpClient(handler), options, NullLogger<HttpTransferAuthorizer>.Instance);
        return (sut, handler);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string json)
       => new(status) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    [Fact]
    public async Task IsAuthorized_CallsConfiguredUrlWithGet()
    {
        var (sut, handler) = Create(_ => Json(HttpStatusCode.OK, """{"status":"success","data":{"authorization":true}}"""));

        await sut.IsAuthorizedAsync(Payer, Payee, 10m);

        var call = handler.Calls.Should().ContainSingle().Subject;
        call.Method.Should().Be(HttpMethod.Get);
        call.Url.Should().Be(Url);
    }

    [Fact]
    public async Task IsAuthorized_AuthorizationTrue_ReturnsTrue()
    {
        var (sut, _) = Create(_ => Json(HttpStatusCode.OK, """{"status":"success","data":{"authorization":true}}"""));

        (await sut.IsAuthorizedAsync(Payer, Payee, 10m)).Should().BeTrue();
    }

    [Fact]
    public async Task IsAuthorized_AuthorizationFalse_ReturnsFalse()
    {
        var (sut, _) = Create(_ => Json(HttpStatusCode.OK, """{"status":"fail","data":{"authorization":false}}"""));

        (await sut.IsAuthorizedAsync(Payer, Payee, 10m)).Should().BeFalse();
    }

    [Fact]
    public async Task IsAuthorized_Forbidden_ReturnsFalse()
    {
        var (sut, _) = Create(_ => Json(HttpStatusCode.Forbidden, """{"status":"fail","data":{"authorization":false}}"""));

        (await sut.IsAuthorizedAsync(Payer, Payee, 10m)).Should().BeFalse();
    }

    [Fact]
    public async Task IsAuthorized_ResponseWithoutAuthorizationField_ReturnsFalse()
    {
        var (sut, _) = Create(_ => Json(HttpStatusCode.OK, "{}"));

        (await sut.IsAuthorizedAsync(Payer, Payee, 10m)).Should().BeFalse();
    }

    [Fact]
    public async Task IsAuthorized_ServerError_ThrowsUnavailableKeepingInnerException()
    {
        var (sut, _) = Create(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        Func<Task> act = () => sut.IsAuthorizedAsync(Payer, Payee, 10m);

        (await act.Should().ThrowAsync<ExternalServiceUnavailableException>())
            .Which.InnerException.Should().NotBeNull();
    }

    [Fact]
    public async Task IsAuthorized_MalformedJson_ThrowsUnavailable()
    {
        var (sut, _) = Create(_ => Json(HttpStatusCode.OK, "not json"));

        Func<Task> act = () => sut.IsAuthorizedAsync(Payer, Payee, 10m);

        await act.Should().ThrowAsync<ExternalServiceUnavailableException>();
    }

    [Fact]
    public async Task IsAuthorized_NetworkFailure_ThrowsUnavailable()
    {
        var (sut, _) = Create(_ => throw new HttpRequestException("connection refused"));

        Func<Task> act = () => sut.IsAuthorizedAsync(Payer, Payee, 10m);

        await act.Should().ThrowAsync<ExternalServiceUnavailableException>();
    }

    [Fact]
    public async Task IsAuthorized_CallerCancels_PropagatesCancellationInsteadOfUnavailable()
    {
        var (sut, _) = Create(_ => Json(HttpStatusCode.OK, """{"data":{"authorization":true}}"""));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Func<Task> act = () => sut.IsAuthorizedAsync(Payer, Payee, 10m, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
