using Microsoft.EntityFrameworkCore;
using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Domain.Entities;
using System.Reflection.Metadata;

namespace SimplifiedPayments.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(int id, CancellationToken ct = default)
        => db.Users.Include(user => user.Wallet)
            .FirstOrDefaultAsync(user => user.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await db.Users.AddAsync(user, ct);

    public Task<bool> ExistsByDocumentAsync(string document, CancellationToken ct = default)
        => db.Users.AnyAsync(user => user.Document == document, ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => db.Users.AnyAsync(user => user.Email == email, ct);
}
