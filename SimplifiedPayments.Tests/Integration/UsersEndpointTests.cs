using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimplifiedPayments.Controllers.Contracts;
using SimplifiedPayments.Domain.Enums;
using SimplifiedPayments.Infrastructure.Persistence;
using SimplifiedPayments.Tests.Helpers;
using System.Net;
using System.Net.Http.Json;

namespace SimplifiedPayments.Tests.Integration;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration")]
public class UsersEndpointTests(ApiFixture api)
{
    private HttpClient Client => api.Client;
    private const string route = "/api/users/v1";

    private async Task<UserResponse> CreateCommonUserAsync(string? email = null, string? document = null)
    {
        var response = await Client.PostAsJsonAsync(route, UserRequests.Common(email, document));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<UserResponse>(TestJson.options))!;
    }

    [Fact]
    public async Task Post_CommonUser_Returns201AndPersistsUserWithEmptyWallet()
    {
        string email = UserRequests.NewEmail();
        string cpf = DocumentGenerator.Cpf();

        var response = await Client.PostAsJsonAsync(route, UserRequests.Common(email, cpf));
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<UserResponse>(TestJson.options);
        content.Should().NotBeNull();
        content.Id.Should().BeGreaterThan(0);
        content.FullName.Should().Be("Carlos Almeida");
        content.Email.Should().Be(email);
        content.Document.Should().Be(cpf);
        content.Type.Should().Be(UserType.Common);

        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = await db.Users.Include(u => u.Wallet).SingleAsync(u => u.Email == email);
        user.Wallet.Balance.Should().Be(0m);
        user.PasswordHash.Should().StartWith("$2").And.NotContain("Senha@123");

        var storedType = await db.Database
            .SqlQuery<string>($"select user_type as [Value] from dbo.tb_users where email = {email}")
            .SingleAsync();
        storedType.Should().Be("COMMON");
    }

    [Fact]
    public async Task Post_ShopkeeperWithFormattedCnpj_Returns201AndStoresOnlyDigits()
    {
        var cnpj = DocumentGenerator.Cnpj();
        var formatted = $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..]}";
        var email = UserRequests.NewEmail();

        var response = await Client.PostAsJsonAsync("/api/users/v1", UserRequests.Shopkeeper(email, formatted));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<UserResponse>(TestJson.options);
        body!.Document.Should().Be(cnpj);
        body.Type.Should().Be(UserType.Shopkeeper);

        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var storedType = await db.Database
            .SqlQuery<string>($"select user_type as [Value] from dbo.tb_users where email = {email}")
            .SingleAsync();
        storedType.Should().Be("SHOPKEEPER");
    }

    [Fact]
    public async Task Post_DuplicateEmailWithDifferentCase_Returns409()
    {
        var email = UserRequests.NewEmail();
        (await Client.PostAsJsonAsync(route, UserRequests.Common(email))).EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync(route, UserRequests.Common(email.ToUpperInvariant()));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.Should().Be(409);
        problem.Detail.Should().Be("Email already registered!");
    }

    [Fact]
    public async Task Post_DuplicateDocumentWithDifferentFormatting_Returns409()
    {
        var cpf = DocumentGenerator.Cpf();
        var formatted = $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}";
        (await Client.PostAsJsonAsync(route, UserRequests.Common(document: cpf))).EnsureSuccessStatusCode();

        var response = await Client.PostAsJsonAsync(route, UserRequests.Common(document: formatted));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Detail.Should().Be("Document already registered!");
    }

    [Fact]
    public async Task Post_InvalidCpf_Returns422()
    {
        var response = await Client.PostAsJsonAsync(route, UserRequests.Common(document: "111.111.111-11"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Post_CommonUserWithCnpj_Returns422()
    {
        var response = await Client.PostAsJsonAsync(route, UserRequests.Common(document: DocumentGenerator.Cnpj()));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Post_WeakPassword_Returns422()
    {
        var response = await Client.PostAsJsonAsync(route, UserRequests.Common(password: "123"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Post_MissingDocument_Returns400()
    {
        var body = new { fullName = "Fulano", email = UserRequests.NewEmail(), password = "Senha@123", type = "Common" };

        var response = await Client.PostAsJsonAsync(route, body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_NumericUserType_Returns400()
    {
        var body = new
        {
            fullName = "Fulano",
            document = DocumentGenerator.Cpf(),
            email = UserRequests.NewEmail(),
            password = "Senha@123",
            type = 99
        };

        var response = await Client.PostAsJsonAsync(route, body);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_ConcurrentRequestsWithSameEmail_CreatesExactlyOneUser()
    {
        var email = UserRequests.NewEmail();

        var responses = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(_ => Client.PostAsJsonAsync(route, UserRequests.Common(email))));

        responses.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(7);

        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Users.CountAsync(u => u.Email == email)).Should().Be(1);
    }

    [Fact]
    public async Task Get_ExistingUser_Returns200WithUserData()
    {
        var email = UserRequests.NewEmail();
        var document = DocumentGenerator.Cpf();
        
        var user = UserRequests.Common(email, document);
        var response = await Client.PostAsJsonAsync(route, user);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<UserResponse>(TestJson.options);
        content.Should().NotBeNull();

        var request = await Client.GetAsync($"/api/users/v1/{content.Id}");
        request.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await request.Content.ReadFromJsonAsync<UserResponse>(TestJson.options);

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.FullName.Should().Be("Carlos Almeida");
        result.Document.Should().Be(document);
        result.Type.Should().Be(UserType.Common);
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task Get_UnknownUser_Return404()
    {
        var response = await Client.GetAsync($"{route}/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Status.Should().Be(404);
        problem.Detail.Should().Be($"User with id: 9999 not found!");
    }

    [Fact]
    public async Task Get_NonNUmericId_Returns404()
    {
        var response = await Client.GetAsync($"{route}/abc");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_ValidData_UpdatesProfileAndKeepsDocumentAndType()
    {
        var cpf = DocumentGenerator.Cpf();
        var newEmail = UserRequests.NewEmail();
        var created = await CreateCommonUserAsync(document: cpf);

        var response = await Client.PutAsJsonAsync(
            $"{route}/{created.Id}",
            new { fullName = "  Novo Nome  ", email = newEmail.ToUpperInvariant() });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var get = await Client.GetAsync($"{route}/{created.Id}");
        var user = await get.Content.ReadFromJsonAsync<UserResponse>(TestJson.options);
        user.Should().Be(new UserResponse(created.Id, "Novo Nome", cpf, newEmail, UserType.Common));
    }

    [Fact]
    public async Task Put_SameEmailAsItself_Returns200()
    {
        var email = UserRequests.NewEmail();
        var created = await CreateCommonUserAsync(email);

        var response = await Client.PutAsJsonAsync($"{route}/{created.Id}", new { fullName = "Outro Nome", email });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Put_EmailOfAnotherUser_Returns409()
    {
        var other = await CreateCommonUserAsync();
        var created = await CreateCommonUserAsync();

        var response = await Client.PutAsJsonAsync($"{route}/{created.Id}", new { fullName = "Fulano", email = other.Email });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Put_UnknownUser_Returns404()
    {
        var response = await Client.PutAsJsonAsync($"{route}/999999",
            new { fullName = "Fulano", email = UserRequests.NewEmail() });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_InvalidEmail_Returns422()
    {
        var created = await CreateCommonUserAsync();

        var response = await Client.PutAsJsonAsync($"{route}/{created.Id}", new { fullName = "Fulano", email = "not-an-email" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Put_MissingFullName_Returns400()
    {
        var created = await CreateCommonUserAsync();

        var response = await Client.PutAsJsonAsync($"{route}/{created.Id}", new { email = UserRequests.NewEmail() });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_ExistingUser_Returns204AndRemovesUserAndWallet()
    {
        var created = await CreateCommonUserAsync();

        var response = await Client.DeleteAsync($"{route}/{created.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Client.GetAsync($"{route}/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);

        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Wallets.AnyAsync(w => w.UserId == created.Id)).Should().BeFalse();
    }

    [Fact]
    public async Task Delete_SameUserTwice_SecondCallReturns404()
    {
        var created = await CreateCommonUserAsync();

        (await Client.DeleteAsync($"{route}/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await Client.DeleteAsync($"{route}/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
