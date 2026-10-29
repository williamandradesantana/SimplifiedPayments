namespace SimplifiedPayments.Application.UseCases.Users;

public sealed record UpdateUserCommand(
    string FullName, string Email
)
{
}
