using SimplifiedPayments.Domain.Exceptions;

namespace SimplifiedPayments.Domain.Entities;

public sealed class Transfer
{
    public int Id { get; private set; }
    public int PayerId { get; private set; }
    public int PayeeId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Transfer(){}

    public static void EnsureCanBeMade(User payer, User payee, decimal amount)
    {
        ArgumentNullException.ThrowIfNull(payer);
        ArgumentNullException.ThrowIfNull(payee);

        Wallet.EnsureValidAmount(amount);

        if (payer.Id == payee.Id) throw new SameUserTransferException();
        payer.EnsureCanSendMoney();

        if (!payer.Wallet.HasSufficientBalance(amount)) 
            throw new InsufficientBalanceException();
    }

    public static Transfer Create(User payer, User payee, decimal amount, DateTime createdAt)
    {
        EnsureCanBeMade(payer, payee, amount);

        payer.Wallet.Debit(amount);
        payee.Wallet.Credit(amount);

        return new Transfer
        {
            PayerId = payer.Id,
            PayeeId = payee.Id,
            Amount = amount,
            CreatedAt = createdAt
        };
    }
}
