using SimplifiedPayments.Domain.Exceptions;

namespace SimplifiedPayments.Domain.ValueObjects;

public sealed record Cpf
{
    public string Value { get; }

    private Cpf(string value) => Value = value;
    
    public static Cpf Create(string? input)
    {
        var digits = DigitsHelper.OnlyDigits(input);

        if (
            digits.Length != 11 || DigitsHelper.AllEqual(digits) ||
            digits[9] - '0' != CheckDigit(digits, 9) ||
            digits[10] - '0' != CheckDigit(digits, 10)
        ) throw new InvalidDocumentException("Invalid CPF.");

        return new Cpf(digits);
    }

    public static int CheckDigit(string digits, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
            sum += (digits[i] - '0') * (length + 1 - i);
        var rest = sum * 10 % 11;
        return rest == 10 ? 0 : rest;
    }
}