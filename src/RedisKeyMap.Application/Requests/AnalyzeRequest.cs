namespace RedisKeyMap.Application.Requests;

public sealed record AnalyzeRequest(string InputPath, string SnapshotPath, string ReportPath, string? SourceLabel = null, long? MaxKeys = null, bool PreserveWhitespace = false, bool IncludeEmptyLines = false);
