using FluentAssertions;
using System.Net;

namespace SimplifiedPayments.Tests.Integration;

[Collection(ApiCollection.Name)]
[Trait("Category", "Integration")]
public class DocumentationTests(ApiFixture api)
{
    [Fact]
    public async Task SwaggerDocument_IsServedWithApiTitle()
    {
        var response = await api.Client.GetAsync("/swagger/v1/swagger.json");
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("Simplified Payments API");
    }
}
