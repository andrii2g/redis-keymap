using System.Collections.Immutable;

namespace RedisKeyMap.Core.Models;

public sealed record DiffSummary(int Info, int Warnings, int Errors);
public sealed record DiffResult(bool IsCompatible, ImmutableArray<string> CompatibilityErrors, string OldSnapshotDescription, string NewSnapshotDescription, ImmutableArray<DiffChange> Changes, DiffSummary Summary);
