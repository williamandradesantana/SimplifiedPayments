namespace SimplifiedPayments.Tests.Helpers;

internal static class UserRequests
{
    public static string NewEmail() => $"{Guid.NewGuid():N}@example.com";

    public static object Common(string? email = null, string? document = null, string password = "Senha@123")
        => new
        {
            fullName = "Carlos Almeida",
            document = document ?? DocumentGenerator.Cpf(),
            email = email ?? NewEmail(),
            password,
            type = "Common"
        };

    public static object Shopkeeper(string? email = null, string? document = null)
        => new
        {
            fullName = "Loja do Zé",
            document = document ?? DocumentGenerator.Cnpj(),
            email = email ?? NewEmail(),
            password = "Senha@123",
            type = "Shopkeeper"
        };
}
