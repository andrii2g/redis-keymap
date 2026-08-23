using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text.Json;
using RedisKeyMap.Core.Normalization;

namespace RedisKeyMap.Core.Utilities;

public sealed record NormalizationRuleFingerprint(string Name, string Pattern, string Replacement, bool IgnoreCase);
public sealed record NormalizationFingerprint(string Delimiter, ImmutableArray<NormalizationRuleFingerprint> CustomRules, ImmutableArray<string> CollapseMarkers, int AlgorithmVersion);

public static class CanonicalConfigHasher
{
    public static string Compute(NormalizationOptions options)
    {
        NormalizationFingerprint value = new(
            options.Delimiter,
            options.CustomRules.Select(rule => new NormalizationRuleFingerprint(rule.Name, rule.Pattern, rule.Replacement, rule.IgnoreCase)).ToImmutableArray(),
            options.LogicalTree.CollapseMarkers,
            1);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(value, NormalizationFingerprintJsonContext.Default.NormalizationFingerprint);
        return Convert.ToHexString(SHA256.HashData(json)).ToLowerInvariant();
    }
}
