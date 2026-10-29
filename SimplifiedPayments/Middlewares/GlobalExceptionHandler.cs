using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimplifiedPayments.Application.Exceptions;
using SimplifiedPayments.Domain.Exceptions;

namespace SimplifiedPayments.Middlewares;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            InvalidRequestException => (StatusCodes.Status422UnprocessableEntity, "Invalid request"),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Business rule violation"),
            ResourceNotFoundException => (StatusCodes.Status404NotFound, "Entity not found"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
        
        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception");
        else
            logger.LogWarning("Request rejected: {Title} - {Message}", title, exception.Message);

        context.Response.StatusCode = status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == StatusCodes.Status500InternalServerError ? null : exception.Message
            }
        });
    }
}