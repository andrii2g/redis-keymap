using RedisKeyMap.Application.Abstractions;
using RedisKeyMap.Application.Requests;
using RedisKeyMap.Application.Results;
using RedisKeyMap.Core.Diffing;

namespace RedisKeyMap.Application.UseCases;

public sealed class DiffSnapshotsUseCase(ISnapshotReader reader, IDiffReportWriter writer)
{
    public async Task<OperationResult> ExecuteAsync(DiffRequest request, CancellationToken cancellationToken)
    {
        var oldSnapshot = await reader.ReadAsync(request.OldSnapshotPath, cancellationToken).ConfigureAwait(false);
        var newSnapshot = await reader.ReadAsync(request.NewSnapshotPath, cancellationToken).ConfigureAwait(false);
        var diff = new SnapshotComparer().Compare(oldSnapshot, newSnapshot, request.AllowConfigMismatch, request.AllowSourceMismatch, request.CountChangeThreshold);
        if (!diff.IsCompatible)
        {
            return new(OperationStatus.OperationalFailure, Diff: diff, Message: string.Join(Environment.NewLine, diff.CompatibilityErrors));
        }

        await writer.WriteAsync(diff, null, request.ReportPath, cancellationToken).ConfigureAwait(false);
        return new(OperationStatus.Success, Diff: diff);
    }
}
