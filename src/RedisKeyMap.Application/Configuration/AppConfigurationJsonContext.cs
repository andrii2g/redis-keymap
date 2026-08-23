using System.Text.Json;
using System.Text.Json.Serialization;

namespace RedisKeyMap.Application.Configuration;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(AppConfiguration))]
public sealed partial class AppConfigurationJsonContext : JsonSerializerContext;
