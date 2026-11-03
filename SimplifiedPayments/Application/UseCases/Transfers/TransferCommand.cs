namespace SimplifiedPayments.Application.UseCases.Transfers;

public sealed record TransferCommand(int PayerId, int PayeeId, decimal Amount);
