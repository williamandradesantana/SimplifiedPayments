using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Application.Exceptions;
using SimplifiedPayments.Domain.Entities;

namespace SimplifiedPayments.Application.UseCases.Users;

public sealed class UpdateUserHandler(
    IUserRepository repository, IUnitOfWork unitOfWork
    )
{
    public async Task<GetOneUserResult> HandleAsync(
        int id, UpdateUserCommand command, CancellationToken ct)
    {
        User user = await repository.GetByIdAsync(id, ct) ?? 
            throw new ResourceNotFoundException($"User with id: {id} not found!");
        
        user.UpdateProfile(command.FullName, command.Email);
        await unitOfWork.SaveChangesAsync(ct);

        return new GetOneUserResult(user.Id, user.FullName, user.Document, user.Email, user.Type);
    }
}
