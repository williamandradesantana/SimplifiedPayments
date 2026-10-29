using Microsoft.AspNetCore.Mvc;
using SimplifiedPayments.Application.UseCases.Users;
using SimplifiedPayments.Controllers.Contracts;

namespace SimplifiedPayments.Controllers;

[ApiController]
[Route("api/users/v1")]
public sealed class UsersController(
    CreateUserHandler createUser, GetOneUserHandler getOneUser,
    UpdateUserHandler updateUser, DeleteUserHandler deleteUser
    ) 
    : ControllerBase
{

    [HttpGet("{id:int}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetOneUser(
        int id, CancellationToken ct
    )
    {
        GetOneUserResult result = await getOneUser.HandleAsync(id, ct);
        return StatusCode(StatusCodes.Status200OK, UserResponse.From(result));
    }

    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        CreateUserRequest request, CancellationToken ct
    )
    {
        CreateUserCommand command = new CreateUserCommand(
            request.FullName, request.Document, request.Email, request.Password, request.Type!.Value
        );
        CreateUserResult result = await createUser.HandleAsync(command, ct);

        return StatusCode(StatusCodes.Status201Created, UserResponse.From(result));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request, CancellationToken ct)
    {
        UpdateUserCommand command = new UpdateUserCommand(request.FullName, request.Email);
        GetOneUserResult result = await updateUser.HandleAsync(id, command, ct);
        return StatusCode(StatusCodes.Status200OK, UserResponse.From(result));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken ct)
    {
        await deleteUser.HandleAsync(id, ct);
        return StatusCode(StatusCodes.Status204NoContent);
    }
}