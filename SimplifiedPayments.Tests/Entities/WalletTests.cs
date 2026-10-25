using FluentAssertions;
using SimplifiedPayments.Domain.Entities;
using SimplifiedPayments.Domain.Exceptions;
using SimplifiedPayments.Tests.Helpers;

namespace SimplifiedPayments.Tests.Entities;

public class WalletTests
{
    public static TheoryData<decimal> InvalidAmounts => new()
    {
        0m, -1m, -0.01m, 10.123m
    };

    public static Wallet NewWallet() => UserFactory.Common().Wallet;

    [Fact]
    public void NewWallet_StartsWithZeroBalance()
    {
        NewWallet().Balance.Should().Be(0m);
    }

    [Fact]
    public void Credit_IncreasesBalance()
    {
        var wallet = NewWallet();
        wallet.Credit(100m);
        wallet.Credit(50.25m);
        wallet.Balance.Should().Be(150.25m);
    }

    [Fact]
    public void Debit_DecreasesBalance()
    {
        var wallet = NewWallet();
        wallet.Credit(100m);
        wallet.Debit(30.50m);
        wallet.Balance.Should().Be(69.50m);
    }

    [Fact]
    public void Debit_ExactBalance_LeavesZero()
    {
        var wallet = NewWallet();
        wallet.Credit(100.00m);
        wallet.Debit(100.00m);
        wallet.Balance.Should().Be(0m);
    }

    [Fact]
    public void Debit_InsufficientBalance_ThrowsAndKeepsBalance()
    {
        var wallet = NewWallet();
        wallet.Credit(100.00m);

        Action act = () => wallet.Debit(120.00m);
        act.Should().Throw<InsufficientBalanceException>().WithMessage("Insufficient balance.");
        wallet.Balance.Should().Be(100.00m);
    }

    [Theory]
    [MemberData(nameof(InvalidAmounts))]
    public void Credit_InvalidAmount_Throws(decimal amount)
    {
        Action act = () => NewWallet().Credit(amount);
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [MemberData(nameof(InvalidAmounts))]
    public void Debit_InvalidAmount_Throws(decimal amount)
    {
        var wallet = NewWallet();
        wallet.Credit(100m);

        Action act = () => wallet.Debit(amount);
        act.Should().Throw<DomainException>();
    }
}
