using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SimplifiedPayments.Domain.Exceptions;
using SimplifiedPayments.Domain.ValueObjects;

namespace SimplifiedPayments.Tests.ValueObjects;

public class CnpjTests
{
    [Theory]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")]
    [InlineData("29141777000158")]
    public void Create_ValidCnpj_StoresOnlyDigits(string input)
    {
        var cnpj = Cnpj.Create(input);
        cnpj.Value.Should().HaveLength(14);
        cnpj.Value.Should().MatchRegex(@"^\d{14}$");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("00000000000000")]   // dígitos repetidos
    [InlineData("11222333000182")]   // dígito verificador errado
    [InlineData("52998224725")]      // um CPF não é um CNPJ
    public void Create_InvalidCnpj_Throws(string? input)
    {
        Action act = () => Cnpj.Create(input);
        act.Should().Throw<InvalidDocumentException>().WithMessage("Invalid CNPJ.");
    }

    [Fact]
    public void Create_SameCnpjWithDifferentFormatting_AreEqual()
    {
        Cnpj.Create("11222333000181").Should().Be(Cnpj.Create("11.222.333/0001-81"));
    }
}
