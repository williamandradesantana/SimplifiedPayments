using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Application.Exceptions;
using SimplifiedPayments.Domain.Entities;

namespace SimplifiedPayments.Application.UseCases.Users;

public sealed class DeleteUserHandler(IUserRepository repository, IUnitOfWork unitOfWork)
{
    public async Task HandleAsync(int id, CancellationToken ct)
    {
        User user = await repository.GetByIdAsync(id, ct) ?? 
            throw new ResourceNotFoundException($"User with id: {id} not found!");

        repository.Remove(user);
        await unitOfWork.SaveChangesAsync(ct);
    }
}