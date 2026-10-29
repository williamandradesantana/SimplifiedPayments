using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimplifiedPayments.Infrastructure.Persistence;
using SimplifiedPayments.Tests.Helpers;

namespace SimplifiedPayments.Tests.Integration;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration")]
public class CorsTests(ApiFixture api)
{
    private Task<HttpResponseMessage> PostAsync(string? origin, string email)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/users/v1")
        {
            Content = JsonContent.Create(UserRequests.Common(email))
        };

        if (origin is not null)
            request.Headers.Add("Origin", origin);

        return api.Client.SendAsync(request);
    }

    [Fact]
    public async Task Post_FromDisallowedOrigin_Returns403AndDoesNotReachTheController()
    {
        var email = UserRequests.NewEmail();

        var response = await PostAsync("http://localhost:3001", email);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Users.AnyAsync(u => u.Email == email)).Should().BeFalse();
    }

    [Fact]
    public async Task Post_FromAllowedOrigin_ReturnsCorsHeader()
    {
        var response = await PostAsync(ApiFixture.AllowedOrigin, UserRequests.NewEmail());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().ContainSingle(ApiFixture.AllowedOrigin);
    }

    [Fact]
    public async Task Post_WithoutOriginHeader_IsNotBlocked()
    {
        var response = await PostAsync(origin: null, UserRequests.NewEmail());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}