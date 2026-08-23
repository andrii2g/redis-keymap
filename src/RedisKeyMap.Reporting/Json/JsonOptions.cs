using System.Text.Json;
using System.Text.Json.Serialization;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Reporting.Json;

internal static class JsonOptions
{
    public static RedisKeyMapJsonContext Create(bool indented = true)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = indented,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            AllowTrailingCommas = false
        };
        options.Converters.Add(new JsonStringEnumConverter<SourceKind>(JsonNamingPolicy.CamelCase));
        options.Converters.Add(new JsonStringEnumConverter<DuplicateHandling>(JsonNamingPolicy.CamelCase));
        options.Converters.Add(new JsonStringEnumConverter<FindingSeverity>(JsonNamingPolicy.CamelCase));
        options.Converters.Add(new JsonStringEnumConverter<ChangeKind>(JsonNamingPolicy.CamelCase));
        return new(options);
    }
}
