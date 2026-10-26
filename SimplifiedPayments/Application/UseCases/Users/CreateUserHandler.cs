using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Application.Exceptions;
using SimplifiedPayments.Domain.Entities;
using System.Text;

namespace SimplifiedPayments.Application.UseCases.Users;

public sealed class CreateUserHandler(
    IUserRepository users, 
    IUnitOfWork unitOfWork, 
    IPasswordHasher passwordHasher
   )
{
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 72;

    public async Task<CreateUserResult> HandleAsync(CreateUserCommand command, CancellationToken ct = default)
    {
        ValidPassword(command.Password);

        User user = User.Create(
            command.FullName, command.Document, 
            command.Email, 
            passwordHasher.Hash(command.Password), 
            command.Type
        );

        if (await users.ExistsByDocumentAsync(user.Document, ct))
            throw new ConflictException("Document already registered!");
        
        if (await users.ExistsByEmailAsync(user.Email, ct))
            throw new ConflictException("Email already registered!");

        await users.AddAsync(user, ct);
        await unitOfWork.SaveChangesAsync(ct);
        
        return new CreateUserResult(user.Id, user.FullName, user.Document, user.Email, user.Type);
    }

    private static void ValidPassword(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            throw new InvalidRequestException($"Password must have at least {MinPasswordLength}");

        if (Encoding.UTF8.GetByteCount(password) > MaxPasswordLength)
            throw new InvalidRequestException($"Passwor must have at most {MaxPasswordLength}");
    }
}
