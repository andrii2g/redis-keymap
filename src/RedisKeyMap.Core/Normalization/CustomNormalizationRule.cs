namespace RedisKeyMap.Core.Normalization;

public sealed record CustomNormalizationRule(string Name, string Pattern, string Replacement, bool IgnoreCase = false);
