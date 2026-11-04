using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Domain.Entities;

namespace SimplifiedPayments.Infrastructure.Persistence.Repositories;

public class TransferRepository(AppDbContext db) : ITransferRepository
{
    public async Task AddAsync(Transfer transfer, CancellationToken ct = default)
        => await db.Transfers.AddAsync(transfer, ct);
}