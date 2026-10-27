using Microsoft.AspNetCore.Mvc;
using SimplifiedPayments.Application.UseCases.Users;
using SimplifiedPayments.Controllers.Contracts;

namespace SimplifiedPayments.Controllers;

[ApiController]
[Route("api/users/v1")]
public sealed class UsersController(CreateUserHandler createUser) 
    : ControllerBase
{

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
}