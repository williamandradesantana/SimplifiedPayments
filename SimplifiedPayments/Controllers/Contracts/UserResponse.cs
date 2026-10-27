using SimplifiedPayments.Application.UseCases.Users;
using SimplifiedPayments.Domain.Enums;

namespace SimplifiedPayments.Controllers.Contracts;

public sealed record UserResponse(
    int id, string FullName, string Document,
    string Email, UserType Type
)
{
    public static UserResponse From(CreateUserResult result)
        => new(
            result.Id, result.FullName, result.Document, 
            result.Email, result.Type
        );
}
