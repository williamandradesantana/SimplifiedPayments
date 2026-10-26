using FluentAssertions;
using SimplifiedPayments.Security;

namespace SimplifiedPayments.Tests.Security;

public class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [Fact]
    public void Hash_ThenVerify_Succeeds()
    {
        var hash = _hasher.Hash("password");

        hash.Should().NotBe("password");
        _hasher.Verify("password", hash).Should().BeTrue();
        _hasher.Verify("outra-senha", hash).Should().BeFalse();
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        _hasher.Hash("Senha@123").Should().NotBe(_hasher.Hash("Senha@123"));
    }

    [Fact]
    public void Verify_SeedHash_AcceptsDevPassword()
    {
        const string seedHash = "$2b$11$E0BqbywCf813.dps0SuVFOQN9NnbrIL0bzRBjzYqKfjygNsRBJvPe";

        _hasher.Verify("Dev@12345", seedHash).Should().BeTrue();
    }
}
