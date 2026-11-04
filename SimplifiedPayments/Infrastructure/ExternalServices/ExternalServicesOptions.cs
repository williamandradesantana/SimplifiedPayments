using System.ComponentModel.DataAnnotations;

namespace SimplifiedPayments.Infrastructure.ExternalServices;

public sealed class ExternalServicesOptions
{
    public const string SectionName = "ExternalServices";

    [Required, Url]
    public string AuthorizerUrl { get; init; } = null!;

    [Required, Url]
    public string NotifierUrl { get; init; } = null!;
}
