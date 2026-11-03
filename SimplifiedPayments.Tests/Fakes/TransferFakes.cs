using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Domain.Entities;

namespace SimplifiedPayments.Tests.Fakes
{
    internal class TransferFakes : ITransferRepository
    {
        public List<Transfer> Transfers { get; } = [];
        public Task AddAsync(Transfer transfer, CancellationToken ct = default)
        {
            Transfers.Add(transfer);
            return Task.CompletedTask;
        }
    }

    internal sealed class FakeTransferAuthorizer : ITransferAuthorizer
    {
        public bool Result { get; set; } = true;
        public Exception? ExceptionToThrow { get; set; }
        public int CallCount { get; private set; }

        public Task<bool> IsAuthorizedAsync(User payer, User payee, decimal amount, CancellationToken ct = default)
        {
            CallCount++;

            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            return Task.FromResult(Result);
        }
    }

    internal sealed class FakeNotificationService : INotificationService
    {
        public List<(string Email, string Message)> Sent { get; } = [];
        public Exception? ExceptionToThrow { get; set; }

        public Task NotifyAsync(string email, string message, CancellationToken ct = default)
        {
            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            Sent.Add((email, message));
            return Task.CompletedTask;
        }
    }

    internal sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
