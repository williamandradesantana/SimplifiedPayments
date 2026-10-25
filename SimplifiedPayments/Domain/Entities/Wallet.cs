using SimplifiedPayments.Domain.Exceptions;

namespace SimplifiedPayments.Domain.Entities;

public class Wallet
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public decimal Balance { get; private set; }

    internal Wallet(){}

    public void Credit(decimal amount)
    {
        EnsureValidAmount(amount);
        Balance += amount;
    }

    public void Debit(decimal amount)
    {
        EnsureValidAmount(amount);
        
        if (Balance < amount) throw new InsufficientBalanceException();
        
        Balance -= amount;
    }

    public static void EnsureValidAmount(decimal amount)
    {
        if (amount <= 0) 
            throw new DomainException("Amount must be greater than zero.");
        
        if (decimal.Round(amount, 2) != amount) 
            throw new DomainException("Amount must have at most 2 decimal places.");
    }
}
