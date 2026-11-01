using FluentAssertions;
using SimplifiedPayments.Domain.Entities;
using SimplifiedPayments.Domain.Exceptions;
using SimplifiedPayments.Tests.Helpers;

namespace SimplifiedPayments.Tests.Entities;

public class TransferTests
{
    private static readonly DateTime Now = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

    public static TheoryData<decimal> InvalidAmounts => new() { 0m, -1m, 10.123m };

    private static User Payer(decimal balance = 100m)
    {
        var user = UserFactory.Common("payer@example.com").WithId(1);
        if (balance > 0) user.Wallet.Credit(balance);
        return user;
    }

    private static User CommonPayee() => UserFactory.Common("payee@example.com").WithId(2);
    private static User ShopkeeperPayee() => UserFactory.Shopkeeper("shop@example.com").WithId(3);

    [Fact]
    public void Create_ValidTransfer_MovesMoneyAndFillsRecord()
    {
        var payer = Payer(100m);
        var payee = CommonPayee();

        var transfer = Transfer.Create(payer, payee, 30.50m, Now);

        payer.Wallet.Balance.Should().Be(69.50m);
        payee.Wallet.Balance.Should().Be(30.50m);
        transfer.PayerId.Should().Be(1);
        transfer.PayeeId.Should().Be(2);
        transfer.Amount.Should().Be(30.50m);
        transfer.CreatedAt.Should().Be(Now);
    }

    [Fact]
    public void Create_ToShopkeeper_Succeeds()
    {
        var payer = Payer(100m);
        var shop = ShopkeeperPayee();

        Transfer.Create(payer, shop, 40m, Now);

        shop.Wallet.Balance.Should().Be(40m);
    }

    [Fact]
    public void Create_EntireBalance_LeavesPayerAtZero()
    {
        var payer = Payer(100m);

        Transfer.Create(payer, CommonPayee(), 100m, Now);

        payer.Wallet.Balance.Should().Be(0m);
    }

    [Fact]
    public void Create_InsufficientBalance_ThrowsAndKeepsBothWallets()
    {
        var payer = Payer(50m);
        var payee = CommonPayee();

        Action act = () => Transfer.Create(payer, payee, 50.01m, Now);

        act.Should().Throw<InsufficientBalanceException>();
        payer.Wallet.Balance.Should().Be(50m);
        payee.Wallet.Balance.Should().Be(0m);
    }

    [Fact]
    public void Create_ShopkeeperAsPayer_Throws()
    {
        var shop = ShopkeeperPayee();
        shop.Wallet.Credit(100m);
        var payee = CommonPayee();

        Action act = () => Transfer.Create(shop, payee, 10m, Now);

        act.Should().Throw<ShopkeeperCannotTransferException>();
        shop.Wallet.Balance.Should().Be(100m);
        payee.Wallet.Balance.Should().Be(0m);
    }

    [Fact]
    public void Create_ToSelf_Throws()
    {
        var payer = Payer(100m);

        Action act = () => Transfer.Create(payer, payer, 10m, Now);

        act.Should().Throw<SameUserTransferException>();
        payer.Wallet.Balance.Should().Be(100m);
    }

    [Theory]
    [MemberData(nameof(InvalidAmounts))]
    public void Create_InvalidAmount_ThrowsAndKeepsBothWallets(decimal amount)
    {
        var payer = Payer(100m);
        var payee = CommonPayee();

        Action act = () => Transfer.Create(payer, payee, amount, Now);

        act.Should().Throw<DomainException>();
        payer.Wallet.Balance.Should().Be(100m);
        payee.Wallet.Balance.Should().Be(0m);
    }

    [Fact]
    public void EnsureCanBeMade_ValidTransfer_DoesNotChangeBalances()
    {
        var payer = Payer(100m);
        var payee = CommonPayee();

        Transfer.EnsureCanBeMade(payer, payee, 30m);

        payer.Wallet.Balance.Should().Be(100m);
        payee.Wallet.Balance.Should().Be(0m);
    }
}
