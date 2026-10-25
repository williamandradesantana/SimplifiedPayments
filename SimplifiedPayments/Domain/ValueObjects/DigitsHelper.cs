namespace SimplifiedPayments.Domain.ValueObjects;

internal static class DigitsHelper
{
    public static string OnlyDigits(string input)
        => new((input ?? string.Empty).Where(char.IsAsciiDigit).ToArray());

    public static bool AllEqual(string digits) => digits.Distinct().Count() == 1;
}
