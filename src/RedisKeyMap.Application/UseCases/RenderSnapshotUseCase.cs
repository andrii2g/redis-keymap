using RedisKeyMap.Application.Abstractions;
using RedisKeyMap.Application.Requests;
using RedisKeyMap.Application.Results;

namespace RedisKeyMap.Application.UseCases;

public sealed class RenderSnapshotUseCase(ISnapshotReader reader, IAnalysisReportWriter writer)
{
    public async Task<OperationResult> ExecuteAsync(RenderRequest request, CancellationToken cancellationToken)
    {
        var snapshot = await reader.ReadAsync(request.SnapshotPath, cancellationToken).ConfigureAwait(false);
        await writer.WriteAsync(snapshot, request.ReportPath, cancellationToken).ConfigureAwait(false);
        return new(OperationStatus.Success, snapshot);
    }
}
