using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Application.Exceptions;
using SimplifiedPayments.Domain.Entities;
using SimplifiedPayments.Domain.Enums;
using SimplifiedPayments.Infrastructure.Persistence;
using SimplifiedPayments.Tests.Helpers;

namespace SimplifiedPayments.Tests.Integration;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration")]
public class PersistenceTests(ApiFixture api)
{
    private static readonly string[] SeedEmails =
    [
        "maria@example.com", "joao@example.com", "ana@example.com",
        "padaria@example.com", "mercado@example.com"
    ];

    [Fact]
    public async Task Migrations_ApplySeedWithUsersAndWallets()
    {
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var seeded = await db.Users.Include(u => u.Wallet)
            .Where(u => SeedEmails.Contains(u.Email))
            .ToListAsync();

        seeded.Should().HaveCount(5);
        seeded.Where(u => u.Type == UserType.Common).Should().OnlyContain(u => u.Wallet.Balance == 1000m);
        seeded.Where(u => u.Type == UserType.Shopkeeper).Should().OnlyContain(u => u.Wallet.Balance == 0m);
    }

    [Fact]
    public async Task SeedPasswordHash_IsAcceptedByBCryptHasher()
    {
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var maria = await db.Users.SingleAsync(u => u.Email == "maria@example.com");

        hasher.Verify("Dev@12345", maria.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task SaveChanges_DuplicateEmail_ThrowsConflictException()
    {
        var email = UserRequests.NewEmail();
        await InsertUserAsync(email, DocumentGenerator.Cpf());

        Func<Task> act = () => InsertUserAsync(email, DocumentGenerator.Cpf());

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task SaveChanges_DuplicateDocument_ThrowsConflictException()
    {
        var cpf = DocumentGenerator.Cpf();
        await InsertUserAsync(UserRequests.NewEmail(), cpf);

        Func<Task> act = () => InsertUserAsync(UserRequests.NewEmail(), cpf);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Database_RejectsNegativeWalletBalance()
    {
        await using var scope = api.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Func<Task> act = () => db.Database.ExecuteSqlInterpolatedAsync(
            $"update dbo.tb_wallets set balance = -1 where user_id = (select id from dbo.tb_users where email = {"maria@example.com"})");

        // 547 = violação de CHECK/FOREIGN KEY (chk_wallet_balance, criado na V2)
        (await act.Should().ThrowAsync<SqlException>()).Where(e => e.Number == 547);
    }

    // Insere direto pelo repository, sem passar pelo "Exists" do handler
    private async Task InsertUserAsync(string email, string cpf)
    {
        await using var scope = api.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await users.AddAsync(User.Create("Fulano de Tal", cpf, email, "hash", UserType.Common));
        await unitOfWork.SaveChangesAsync();
    }
}