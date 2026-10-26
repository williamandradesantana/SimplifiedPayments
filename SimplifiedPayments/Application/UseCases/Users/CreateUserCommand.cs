using SimplifiedPayments.Domain.Enums;

namespace SimplifiedPayments.Application.UseCases.Users;

public sealed record CreateUserCommand(
    string FullName,
    string Document,
    string Email,
    string Password,
    UserType Type
);