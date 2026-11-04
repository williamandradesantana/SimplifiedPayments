using SimplifiedPayments.Application.UseCases.Transfers;

namespace SimplifiedPayments.Controllers.Contracts;

public sealed record TransferResponse(
    long Id, int PayerId, int PayeeId,
    decimal Amount, DateTime CreatedAt
    )
{
    public static TransferResponse From(TransferResult transfer)
    {
        return new TransferResponse(
            transfer.Id, transfer.PayerId, transfer.PayeeId, 
            transfer.Amount, transfer.CreatedAt
        );
    }
}