namespace RedisKeyMap.Core.Models;

public sealed record DiffChange(ChangeKind Kind, FindingSeverity Severity, string Subject, string? OldValue, string? NewValue, string Message);
