using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimplifiedPayments.Tests.Integration;

internal static class TestJson
{
    public static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
