using FluentAssertions;
using SimplifiedPayments.Domain.Entities;
using SimplifiedPayments.Domain.Enums;
using SimplifiedPayments.Domain.Exceptions;
using SimplifiedPayments.Tests.Helpers;
using System.Xml.Linq;

namespace SimplifiedPayments.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_CommonUser_NormalizesData()
    {
        User user = User.Create("  USER ", "529.982.247-25", "user@email.com", "password", UserType.Common);

        user.FullName.Should().Be("USER");
        user.Document.Should().Be("52998224725");
        user.Email.Should().Be("user@email.com");
        user.PasswordHash.Should().Be("password");
        user.Type.Should().Be(UserType.Common);
        user.Wallet.Balance.Should().Be(0m);
    }

    [Fact]
    public void Create_ShopkeeperWithCnpj_Succeeds()
    {
        User shopkeeper = User.Create("Padaria", "11.222.333/0001-81", "padaria@email.com", "password", UserType.Shopkeeper);

        shopkeeper.FullName.Should().Be("Padaria");
        shopkeeper.Document.Should().Be("11222333000181");
        shopkeeper.Email.Should().Be("padaria@email.com");
        shopkeeper.PasswordHash.Should().Be("password");
        shopkeeper.Type.Should().Be(UserType.Shopkeeper);
        shopkeeper.Wallet.Balance.Should().Be(0m);
    }

    [Fact]
    public void Create_CommonUserWithCnpj_Throws()
    {
        Action act = () => User.Create(
            "  USER ", UserFactory.ValidCnpj, "user@email.com", "password", UserType.Common
        );
        act.Should().Throw<InvalidDocumentException>().WithMessage("Invalid CPF.");
    }

    [Fact]
    public void Create_ShopkeeperWithCpf_Throws()
    {
        Action act = () => User.Create(
            "  USER ", UserFactory.ValidCpf, "user@email.com", "password", UserType.Shopkeeper
        );
        act.Should().Throw<InvalidDocumentException>().WithMessage("Invalid CNPJ.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyName_Throws(string? name)
    {
        Action act = () => User.Create(
            name!, UserFactory.ValidCpf, "user@email.com", "password", UserType.Common
        );
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_NameLongerThan150_Throws()
    {
        Action act = () => User.Create(
           new string('a', 151), UserFactory.ValidCpf, "user@email.com", "password", UserType.Common
        );
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("a@")]
    [InlineData("John <john@example.com>")]   // MailAddress aceita nome de exibição
    public void Create_InvalidEmail_Throws(string? email)
    {
        Action act = () => User.Create(
            "USER", UserFactory.ValidCpf, email!, "password", UserType.Common
        );
        act.Should().Throw<DomainException>().WithMessage("Invalid email.");
    }

    [Fact]
    public void Create_EmailLongerThan100_Throws()
    {
        string email = new string('a', 95) + "@x.com";
        Action act = () => User.Create(
            "USER", UserFactory.ValidCpf, email, "password", UserType.Common
        );
        act.Should().Throw<DomainException>().WithMessage("Invalid email.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyPasswordHash_Throws(string? hash)
    {
        Action act = () => User.Create(
            "USER", UserFactory.ValidCpf, hash!, "password", UserType.Common
        );
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CommonUser_CanSendMoney()
    {
        User user = UserFactory.Common();
        user.CanSendMoney.Should().BeTrue();
        user.EnsureCanSendMoney();
    }

    [Fact]
    public void Shopkeeper_CannotSendMoney()
    {
        User user = UserFactory.Shopkeeper();
        user.CanSendMoney.Should().BeFalse();
        
        Action act = () => user.EnsureCanSendMoney();
        act.Should().Throw<ShopkeeperCannotTransferException>().WithMessage("Shopkeepers cannot send transfers.");
    }
}
