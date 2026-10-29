namespace SimplifiedPayments.Tests.Integration;

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}
