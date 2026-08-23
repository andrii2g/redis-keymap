namespace RedisKeyMap.Application.Requests;

public sealed record CheckRequest(string BaselinePath, string CurrentPath, string? ReportPath, bool AllowConfigMismatch = false, bool AllowSourceMismatch = false);
