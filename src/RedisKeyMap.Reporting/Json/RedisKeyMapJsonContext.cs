using System.Text.Json.Serialization;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Reporting.Json;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(Snapshot))]
public sealed partial class RedisKeyMapJsonContext : JsonSerializerContext;
