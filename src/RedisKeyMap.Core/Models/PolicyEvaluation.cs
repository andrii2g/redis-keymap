using System.Collections.Immutable;

namespace RedisKeyMap.Core.Models;

public sealed record PolicyViolation(string RuleId, string Message, string? Subject);
public sealed record PolicyEvaluation(bool Passed, ImmutableArray<PolicyViolation> Violations, ImmutableArray<string> Warnings);
