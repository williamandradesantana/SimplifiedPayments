namespace SimplifiedPayments.Application.UseCases.Transfers;

public sealed record TransferResult(
    long Id, int PayerId, int PayeeId, decimal Amount, DateTime CreatedAt
);