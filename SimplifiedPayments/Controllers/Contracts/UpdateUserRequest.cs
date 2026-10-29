using System.ComponentModel.DataAnnotations;

namespace SimplifiedPayments.Controllers.Contracts;

public sealed class UpdateUserRequest
{
    [Required, StringLength(150)]
    public string FullName { get; init; } = null!;

    [Required, StringLength(100)]
    public string Email { get; init; } = null!;
}
