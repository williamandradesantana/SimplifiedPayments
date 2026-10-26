using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SimplifiedPayments.Domain.Enums;

namespace SimplifiedPayments.Infrastructure.Persistence.Mappings;

public sealed class UserTypeConverter() : ValueConverter<UserType, string>(
    value => ToDatabase(value), value => FromDatabase(value)
)
{
    private static string ToDatabase(UserType type) => type switch
    {
        UserType.Common => "COMMON",
        UserType.Shopkeeper => "SHOPKEEPER",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown user type.")
    };

    private static UserType FromDatabase(string value) => value switch
    {
        "COMMON" => UserType.Common,
        "SHOPKEEPER" => UserType.Shopkeeper,
        _ => throw new InvalidOperationException($"Unknown user type in database: '{value}'.")
    };

}