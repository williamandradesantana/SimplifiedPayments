using SimplifiedPayments.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimplifiedPayments.Controllers.Contracts;

public sealed class CreateUserRequest
{
    [Required, StringLength(150)]
    public string FullName { get; init; } = null!;

    [Required, StringLength(18)]
    public string Document { get; init; } = null!;

    [Required, StringLength(100)]
    public string Email { get; init; } = null!;

    [Required, StringLength(255)]
    public string Password { get; init; } = null!;

    [Required]
    public UserType? Type { get; init; }
}
