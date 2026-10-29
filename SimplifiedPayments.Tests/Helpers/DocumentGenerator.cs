namespace SimplifiedPayments.Tests.Helpers;

internal static class DocumentGenerator
{
    private static readonly int[] CnpjWeights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] CnpjWeights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    public static string Cpf()
    {
        while (true)
        {
            var d = new int[11];
            for (var i = 0; i < 9; i++) d[i] = Random.Shared.Next(10);
            d[9] = CpfCheck(d, 9);
            d[10] = CpfCheck(d, 10);

            var value = string.Concat(d);
            if (value.Distinct().Count() > 1) return value;   // evita 111.111.111-11
        }
    }

    public static string Cnpj()
    {
        while (true)
        {
            var d = new int[14];
            for (var i = 0; i < 12; i++) d[i] = Random.Shared.Next(10);
            d[12] = CnpjCheck(d, CnpjWeights1);
            d[13] = CnpjCheck(d, CnpjWeights2);

            var value = string.Concat(d);
            if (value.Distinct().Count() > 1) return value;
        }
    }

    private static int CpfCheck(int[] d, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++) sum += d[i] * (length + 1 - i);
        var rest = sum * 10 % 11;
        return rest == 10 ? 0 : rest;
    }

    private static int CnpjCheck(int[] d, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < weights.Length; i++) sum += d[i] * weights[i];
        var rest = sum % 11;
        return rest < 2 ? 0 : 11 - rest;
    }
}