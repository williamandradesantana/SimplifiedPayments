using Microsoft.AspNetCore.Mvc;
using SimplifiedPayments.Application.UseCases.Transfers;
using SimplifiedPayments.Controllers.Contracts;

namespace SimplifiedPayments.Controllers;

[ApiController]
[Route("/api/transfers/v1")]
public sealed class TransfersController(TransferHandler transfer) : ControllerBase
{

    [HttpPost]
    [ProducesResponseType<TransferResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreateTransfer(TransferRequest request, CancellationToken ct)
    {
        TransferCommand command = new(request.Payer!.Value, request.Payee!.Value, request.Value!.Value);
        TransferResult result = await transfer.HandleAsync(command, ct);

        return StatusCode(StatusCodes.Status201Created, TransferResponse.From(result));
    }
}