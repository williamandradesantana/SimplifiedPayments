using SimplifiedPayments.Domain.Enums;

namespace SimplifiedPayments.Application.UseCases.Users;

public sealed record GetOneUserResult(
    int Id, string FullName,
    string Document, string Email,
    UserType Type
)
{
}
