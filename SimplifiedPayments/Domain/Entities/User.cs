using SimplifiedPayments.Domain.Enums;
using SimplifiedPayments.Domain.Exceptions;
using SimplifiedPayments.Domain.ValueObjects;
using System.Net;
using System.Net.Mail;

namespace SimplifiedPayments.Domain.Entities;

public class User
{
    public int Id { get; private set; }
    public string FullName { get; private set; } = null!;
    public string Document { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserType Type { get; private set; }
    public Wallet Wallet { get; private set; } = null!;

    public User(){}

    public static User Create(
        string fullName, string document, string email, string passwordHash, UserType type
    )
    {
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Trim().Length > 150)
            throw new DomainException("Full name is required and must have at most 150 characters.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password Hash is required.");

        var normalizedDocument = type == UserType.Common
            ? Cpf.Create(document).Value
            : Cnpj.Create(document).Value;

        return new User
        {
            FullName = fullName.Trim(),
            Document = normalizedDocument,
            Email = NormalizeEmail(email),
            PasswordHash = passwordHash,
            Type = type,
            Wallet = new Wallet()
        };
    }

    public bool CanSendMoney => Type == UserType.Common;

    public void EnsureCanSendMoney()
    {
        if (!CanSendMoney) throw new ShopkeeperCannotTransferException();
    }

    private static string NormalizeEmail(string? email)
    {
        var trimmed = email?.Trim() ?? string.Empty;

        if (trimmed.Length is 0 or > 100
            || !MailAddress.TryCreate(trimmed, out var address)
            || address.Address != trimmed)
            throw new DomainException("Invalid email.");

        return trimmed.ToLowerInvariant();
    }
}