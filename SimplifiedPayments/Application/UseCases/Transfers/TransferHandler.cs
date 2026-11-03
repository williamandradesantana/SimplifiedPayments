using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Application.Exceptions;
using SimplifiedPayments.Domain.Entities;
using System.Globalization;

namespace SimplifiedPayments.Application.UseCases.Transfers;

public sealed class TransferHandler(
    IUserRepository userRepository,
    ITransferRepository transferRepository,
    IUnitOfWork unitOfWork,
    ITransferAuthorizer authorizer,
    INotificationService notifier,
    TimeProvider clock,
    ILogger<TransferHandler> logger)
{
    public async Task<TransferResult> HandleAsync(TransferCommand command, CancellationToken ct = default)
    {
        var payer = await userRepository.GetByIdAsync(command.PayerId, ct) ??
            throw new ResourceNotFoundException($"Payer with id: {command.PayerId} not found!");
        var payee = await userRepository.GetByIdAsync(command.PayeeId, ct) ??
            throw new ResourceNotFoundException($"Payee with id: {command.PayeeId} not found!");

        Transfer.EnsureCanBeMade(payer, payee, command.Amount);

        if (!await authorizer.IsAuthorizedAsync(payer, payee, command.Amount, ct))
            throw new TransferNotAuthorizedException();

        var transfer = Transfer.Create(payer, payee, command.Amount, clock.GetUtcNow().UtcDateTime);

        await transferRepository.AddAsync(transfer, ct);
        await unitOfWork.SaveChangesAsync(ct);

        await NotifyPayeeAsync(payer, payee, transfer);

        return new TransferResult(transfer.Id, transfer.PayerId, transfer.PayeeId, transfer.Amount, transfer.CreatedAt);
    }

    private async Task NotifyPayeeAsync(User payer, User payee, Transfer transfer)
    {
        var amount = transfer.Amount.ToString("N2", CultureInfo.InvariantCulture);

        try
        {
            await notifier.NotifyAsync(
                payee.Email, $"You received {amount} from {payer.FullName}.", CancellationToken.None
            );
        } catch (Exception ex)
        {
            logger.LogWarning(
                ex, "Failed to notify user {UserId} about transfer {TransferId}", payee.Id, transfer.Id
            );
        }
    }
}
