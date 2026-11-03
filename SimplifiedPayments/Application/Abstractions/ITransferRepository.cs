using SimplifiedPayments.Domain.Entities;

namespace SimplifiedPayments.Application.Abstractions;

public interface ITransferRepository
{
    Task AddAsync(Transfer transfer, CancellationToken ct = default);
}