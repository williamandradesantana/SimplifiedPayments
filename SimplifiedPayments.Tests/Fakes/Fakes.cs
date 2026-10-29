using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Domain.Entities;

namespace SimplifiedPayments.Tests.Fakes;

internal class FakeUserRepository : IUserRepository
{
    public List<User> Users { get; } = [];

    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => Task.FromResult(Users.FirstOrDefault(user => user.Id == id));

    public Task AddAsync(User user, CancellationToken ct = default)
    {
        Users.Add(user);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByDocumentAsync(string document, CancellationToken ct = default)
        => Task.FromResult(Users.Any(user => user.Document == document));

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => Task.FromResult(Users.Any(user => user.Email == email));

    public void Remove(User user)
        => Users.Remove(user);
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }
}

internal sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password)
        => $"hashed:{password}";

    public bool Verify(string password, string hash)
        => hash == Hash(password);
}
