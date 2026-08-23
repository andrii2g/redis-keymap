using RedisKeyMap.Application.Abstractions;
using RedisKeyMap.Application.Configuration;
using RedisKeyMap.Application.Requests;
using RedisKeyMap.Application.Results;
using RedisKeyMap.Application.Sources;
using RedisKeyMap.Core.Aggregation;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Application.UseCases;

public sealed class AnalyzeFileUseCase(ISnapshotWriter snapshotWriter, IAnalysisReportWriter reportWriter, TimeProvider timeProvider, string toolVersion)
{
    public async Task<OperationResult> ExecuteAsync(AnalyzeRequest request, AppConfiguration configuration, CancellationToken cancellationToken)
    {
        await using TextFileKeySource source = new(request.InputPath, request.PreserveWhitespace, request.IncludeEmptyLines);
        AnalysisOptions options = new()
        {
            Normalization = configuration.Normalization,
            Privacy = configuration.Privacy,
            DuplicateHandling = configuration.Analysis.DuplicateHandling,
            MaximumDepthFinding = configuration.Analysis.MaximumDepth,
            MaximumNamespacesFinding = configuration.Analysis.MaximumNamespaces,
            MaximumPatternsFinding = configuration.Analysis.MaximumPatterns,
            MaxKeys = request.MaxKeys
        };
        AnalysisResult result = await new KeyspaceAnalyzer().AnalyzeAsync(
            source.ReadAsync(cancellationToken),
            options,
            new SnapshotSource(SourceKind.TextFile, SourceLabel: request.SourceLabel ?? Path.GetFileName(request.InputPath)),
            timeProvider,
            toolVersion,
            cancellationToken).ConfigureAwait(false);
        await snapshotWriter.WriteAsync(result.Snapshot, request.SnapshotPath, cancellationToken).ConfigureAwait(false);
        await reportWriter.WriteAsync(result.Snapshot, request.ReportPath, cancellationToken).ConfigureAwait(false);
        return new(OperationStatus.Success, result.Snapshot);
    }
}
