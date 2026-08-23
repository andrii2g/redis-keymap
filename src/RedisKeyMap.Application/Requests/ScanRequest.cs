namespace RedisKeyMap.Application.Requests;

public sealed record ScanRequest(int Database, string Match, int PageSize, string SnapshotPath, string ReportPath, long? MaxKeys = null, string? SourceLabel = null);
