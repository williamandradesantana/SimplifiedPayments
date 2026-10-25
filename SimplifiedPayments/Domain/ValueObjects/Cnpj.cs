using SimplifiedPayments.Domain.Exceptions;

namespace SimplifiedPayments.Domain.ValueObjects;

public sealed record Cnpj
{
    private static readonly int[] FirstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] SecondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    public string Value { get; }

    private Cnpj(string value) => Value = value;

    public static Cnpj Create(string? input)
    {
        var digits = DigitsHelper.OnlyDigits(input);
        
        if (
            digits.Length != 14 || DigitsHelper.AllEqual(digits) ||
            digits[12] - '0' != CheckDigit(digits, FirstWeights) ||
            digits[13] - '0' != CheckDigit(digits, SecondWeights)
        ) throw new InvalidDocumentException("Invalid CNPJ.");

        return new Cnpj(digits);
    }

    private static int CheckDigit(string digits, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++)
            sum += (digits[i] - '0') * weights[i];

        var rest = sum % 11;
        return rest < 2 ? 0 : 11 - rest;
    }
}
