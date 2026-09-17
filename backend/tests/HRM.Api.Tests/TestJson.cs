using System.Text.Json;
using System.Text.Json.Serialization;

namespace HRM.Api.Tests;

/// <summary>Matches the JsonSerializerOptions the API registers in Program.cs
/// (string enums) so test clients can (de)serialize the same way real callers would.</summary>
public static class TestJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true
    };
}
