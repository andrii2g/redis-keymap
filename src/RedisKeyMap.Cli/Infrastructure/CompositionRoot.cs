using RedisKeyMap.Application.Configuration;
using RedisKeyMap.Application.UseCases;
using RedisKeyMap.Reporting.Json;
using RedisKeyMap.Reporting.Markdown;

namespace RedisKeyMap.Cli.Infrastructure;

public static class CompositionRoot
{
    public static AnalyzeFileUseCase Analyze(AppConfiguration configuration, string version) =>
        new(new SnapshotJsonWriter(), new MarkdownReportWriter(configuration.Report.MaximumPatternRows, configuration.Report.ShowTreeCounts, configuration.Report.MaximumTreeDepth), TimeProvider.System, version);

    public static RenderSnapshotUseCase Render(AppConfiguration configuration) =>
        new(new SnapshotJsonReader(), new MarkdownReportWriter(configuration.Report.MaximumPatternRows, configuration.Report.ShowTreeCounts, configuration.Report.MaximumTreeDepth));

    public static DiffSnapshotsUseCase Diff() => new(new SnapshotJsonReader(), new MarkdownDiffReportWriter());
    public static CheckSnapshotsUseCase Check() => new(new SnapshotJsonReader(), new MarkdownDiffReportWriter());
}
