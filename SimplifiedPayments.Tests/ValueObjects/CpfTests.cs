using FluentAssertions;
using SimplifiedPayments.Domain.Exceptions;
using SimplifiedPayments.Domain.ValueObjects;

namespace SimplifiedPayments.Tests.ValueObjects;

public class CpfTests
{
    [Theory]
    [InlineData("52998224725", "52998224725")]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("  529.982.247-25  ", "52998224725")]
    [InlineData("29141777638", "29141777638")]
    public void Create_ValidCpf_StoresOnlyDigits(string input, string expected)
    {
        var cpf = Cpf.Create(input);

        cpf.Value.Should().Be(expected);
        cpf.Value.Should().HaveLength(11);
        cpf.Value.Should().MatchRegex(@"^\d{11}$");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("11111111111")]
    [InlineData("52998224726")]
    [InlineData("5299822472500")]
    [InlineData("abcdefghijk")]
    public void Create_InvalidCpf_Throws(string input)
    {
        Action act = () => Cpf.Create(input);
        act.Should().Throw<InvalidDocumentException>().WithMessage("Invalid CPF.");
    }

    [Fact]
    public void Create_SameCpfWithDifferentFormatting_AreEqual()
    {
        Cpf.Create("529.982.247-25").Should().Be(Cpf.Create("52998224725"));
    }
}
