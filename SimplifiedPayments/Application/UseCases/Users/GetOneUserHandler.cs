using SimplifiedPayments.Application.Abstractions;
using SimplifiedPayments.Application.Exceptions;
using SimplifiedPayments.Domain.Entities;

namespace SimplifiedPayments.Application.UseCases.Users;

public sealed class GetOneUserHandler(IUserRepository repository)
{
    public async Task<GetOneUserResult> HandleAsync(int id, CancellationToken ct)
    {
        User? user = await repository.GetByIdAsync(id, ct) ?? 
            throw new ResourceNotFoundException($"User with id: {id} not found!");
        return new GetOneUserResult(user.Id, user.FullName, user.Document, user.Email, user.Type);
    }
}