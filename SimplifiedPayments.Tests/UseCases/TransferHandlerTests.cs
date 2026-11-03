using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SimplifiedPayments.Application.Exceptions;
using SimplifiedPayments.Application.UseCases.Transfers;
using SimplifiedPayments.Domain.Entities;
using SimplifiedPayments.Domain.Exceptions;
using SimplifiedPayments.Tests.Fakes;
using SimplifiedPayments.Tests.Helpers;

namespace SimplifiedPayments.Tests.UseCases;

public class TransferHandlerTests
{
    private static readonly DateTimeOffset Now = new(26, 11, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly FakeUserRepository _users = new();
    private readonly TransferFakes _transfers = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakeTransferAuthorizer _authorizer = new();
    private readonly FakeNotificationService _notifier = new();
    private readonly TransferHandler _handler;

    public TransferHandlerTests()
        => _handler = new TransferHandler(
            _users, _transfers, _unitOfWork, _authorizer, _notifier,
            new FixedTimeProvider(Now), NullLogger<TransferHandler>.Instance);

    private User AddPayer(decimal balance = 100m)
    {
        var user = UserFactory.Common("payer@example.com").WithId(1);
        if (balance > 0) user.Wallet.Credit(balance);
        _users.Users.Add(user);
        return user;
    }

    private User AddCommonPayee()
    {
        var user = UserFactory.Common("payee@example.com").WithId(2);
        _users.Users.Add(user);
        return user;
    }

    private User AddShopkeeper(decimal balance = 0m)
    {
        var user = UserFactory.Shopkeeper("shop@example.com").WithId(3);
        if (balance > 0) user.Wallet.Credit(balance);
        _users.Users.Add(user);
        return user;
    }

    [Fact]
    public async Task Handle_ValidTransfer_PersistsMovesMoneyAndNotifiesPayee()
    {
        var payer = AddPayer(100m);
        var payee = AddCommonPayee();

        var result = await _handler.HandleAsync(new TransferCommand(payer.Id, payee.Id, 30.50m));

        result.PayerId.Should().Be(1);
        result.PayeeId.Should().Be(2);
        result.Amount.Should().Be(30.50m);
        result.CreatedAt.Should().Be(Now.UtcDateTime);

        payer.Wallet.Balance.Should().Be(69.50m);
        payee.Wallet.Balance.Should().Be(30.50m);
        _transfers.Transfers.Should().ContainSingle();
        _unitOfWork.SaveCount.Should().Be(1);

        var notification = _notifier.Sent.Should().ContainSingle().Subject;
        notification.Email.Should().Be("payee@example.com");
        notification.Message.Should().Contain("30.50");
    }

    [Fact]
    public async Task Handle_ToShopkeeper_Success()
    {
        AddPayer(100m);
        var shopkeeper = AddShopkeeper();

        await _handler.HandleAsync(new TransferCommand(1, shopkeeper.Id, 40m));

        shopkeeper.Wallet.Balance.Should().Be(40m);
    }

    [Fact]
    public async Task Handle_AuthorizerDenies_ThrowsAndChangesNothing()
    {
        var payer = AddPayer(100m);
        var payee = AddCommonPayee();
        _authorizer.Result = false;

        Func<Task> act = () => _handler.HandleAsync(new TransferCommand(payer.Id, payee.Id, 30m));

        await act.Should().ThrowAsync<TransferNotAuthorizedException>();

        payer.Wallet.Balance.Should().Be(100m);
        payee.Wallet.Balance.Should().Be(0m);
        _transfers.Transfers.Should().BeEmpty();
        _unitOfWork.SaveCount.Should().Be(0);
        _notifier.Sent.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_AuthorizerUnavailable_ThrowsAndChangesNothing()
    {
        var payer = AddPayer(100m);
        AddCommonPayee();
        _authorizer.ExceptionToThrow = new ExternalServiceUnavailableException("Authorizer is unavailable.");

        Func<Task> act = () => _handler.HandleAsync(new TransferCommand(payer.Id, 2, 30m));

        await act.Should().ThrowAsync<ExternalServiceUnavailableException>();
        payer.Wallet.Balance.Should().Be(100m);
        _transfers.Transfers.Should().BeEmpty();
        _unitOfWork.SaveCount.Should().Be(0);
    }


    [Fact]
    public async Task Handle_NotificationFails_TransferStillCompletes()
    {
        var payer = AddPayer(100m);
        var payee = AddCommonPayee();
        _notifier.ExceptionToThrow = new InvalidOperationException("notifier is down");

        var result = await _handler.HandleAsync(new TransferCommand(1, 2, 30m));

        result.Amount.Should().Be(30m);
        payer.Wallet.Balance.Should().Be(70m);
        payee.Wallet.Balance.Should().Be(30m);
        _unitOfWork.SaveCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_InsufficientBalance_ThrowsWithoutCallingAuthorizer()
    {
        AddPayer(10m);
        AddCommonPayee();

        Func<Task> act = () => _handler.HandleAsync(new TransferCommand(1, 2, 50m));

        await act.Should().ThrowAsync<InsufficientBalanceException>();
        _authorizer.CallCount.Should().Be(0);
        _unitOfWork.SaveCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShopkeeperAsPayer_ThrowsWithoutCallingAuthorizer()
    {
        AddShopkeeper(balance: 100m);
        AddCommonPayee();

        Func<Task> act = () => _handler.HandleAsync(new TransferCommand(3, 2, 10m));

        await act.Should().ThrowAsync<ShopkeeperCannotTransferException>();
        _authorizer.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_TransferToSelf_ThrowsWithoutCallingAuthorizer()
    {
        AddPayer(100m);

        Func<Task> act = () => _handler.HandleAsync(new TransferCommand(1, 1, 10m));

        await act.Should().ThrowAsync<SameUserTransferException>();
        _authorizer.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_UnknownPayer_ThrowsNotFound()
    {
        AddCommonPayee();

        Func<Task> act = () => _handler.HandleAsync(new TransferCommand(99, 2, 10m));

        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*Payer*");
    }

    [Fact]
    public async Task Handle_UnknownPayee_ThrowsNotFound()
    {
        AddPayer(100m);

        Func<Task> act = () => _handler.HandleAsync(new TransferCommand(1, 99, 10m));

        await act.Should().ThrowAsync<ResourceNotFoundException>().WithMessage("*Payee*");
    }
}
