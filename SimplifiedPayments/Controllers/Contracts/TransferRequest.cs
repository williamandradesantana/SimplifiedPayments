using System.ComponentModel.DataAnnotations;

namespace SimplifiedPayments.Controllers.Contracts;

public sealed class TransferRequest
{
    [Required, Range(0.01, 1_000_000_000)]
    public decimal? Value { get; init; }

    [Required, Range(1, int.MaxValue)]
    public int? Payer { get; init; }

    [Required, Range(1, int.MaxValue)]
    public int? Payee { get; init; }
}
