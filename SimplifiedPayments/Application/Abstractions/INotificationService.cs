namespace SimplifiedPayments.Application.Abstractions;

public interface INotificationService
{
    Task NotifyAsync(string email, string message, CancellationToken ct = default);
}
