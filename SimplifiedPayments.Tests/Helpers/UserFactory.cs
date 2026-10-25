using SimplifiedPayments.Domain.Entities;
using SimplifiedPayments.Domain.Enums;

namespace SimplifiedPayments.Tests.Helpers;

internal static class UserFactory
{
    public const string ValidCpf = "52998224725";
    public const string ValidCnpj = "11222333000181";

    public static User Common(string email = "maria@example.com")
        => User.Create("Maria Souza", ValidCpf, email, "hash", UserType.Common);

    public static User Shopkeeper(string email = "loja@example.com")
        => User.Create("Padaria Pão Quente", ValidCnpj, email, "hash", UserType.Shopkeeper);
}
