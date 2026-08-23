using System.Text.Json.Serialization;

namespace RedisKeyMap.Core.Utilities;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(NormalizationFingerprint))]
public sealed partial class NormalizationFingerprintJsonContext : JsonSerializerContext;
