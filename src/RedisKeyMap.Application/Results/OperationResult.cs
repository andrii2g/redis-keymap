using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Application.Results;

public sealed record OperationResult(OperationStatus Status, Snapshot? Snapshot = null, DiffResult? Diff = null, PolicyEvaluation? Policy = null, string? Message = null);
