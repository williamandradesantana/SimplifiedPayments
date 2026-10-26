using SimplifiedPayments.Domain.Enums;

namespace SimplifiedPayments.Application.UseCases.Users;


public sealed record CreateUserResult(
    int Id,
    string FullName,
    string Document,
    string Email,
    UserType Type
);
