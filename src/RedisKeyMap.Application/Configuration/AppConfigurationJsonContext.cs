using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace RedisKeyMap.Application.Configuration;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(AppConfiguration))]
public sealed partial class AppConfigurationJsonContext : JsonSerializerContext
{
    protected override JsonSerializerOptions? GeneratedSerializerOptions => throw new NotImplementedException();

    public override JsonTypeInfo? GetTypeInfo(Type type) => throw new NotImplementedException();
}