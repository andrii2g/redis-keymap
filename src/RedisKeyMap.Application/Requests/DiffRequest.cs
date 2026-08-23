namespace RedisKeyMap.Application.Requests;

public sealed record DiffRequest(string OldSnapshotPath, string NewSnapshotPath, string ReportPath, bool AllowConfigMismatch = false, bool AllowSourceMismatch = false, decimal CountChangeThreshold = 0);
