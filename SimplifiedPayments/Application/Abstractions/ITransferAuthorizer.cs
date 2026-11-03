using SimplifiedPayments.Domain.Entities;

namespace SimplifiedPayments.Application.Abstractions;

public interface ITransferAuthorizer
{
    Task<bool> IsAuthorizedAsync(User payer, User payee, decimal amount, CancellationToken ct = default);
}