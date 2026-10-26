using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Application.Exceptions;

namespace SimplifiedPayments.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;
    
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException
            { Number: UniqueIndexViolation or UniqueConstraintViolation })
        {
            throw new ConflictException("A record with the same unique value already exists.");
        }
    }
}
