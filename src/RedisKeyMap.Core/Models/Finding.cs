using System.Collections.Immutable;

namespace RedisKeyMap.Core.Models;

public sealed record Finding(string RuleId, FindingSeverity Severity, string Message, string? Pattern, ImmutableSortedDictionary<string, string> Evidence);
