using RedisKeyMap.Core.Diffing;
using RedisKeyMap.Reporting.Json;
using RedisKeyMap.Reporting.Markdown;

namespace RedisKeyMap.Reporting.Tests;

public sealed class SnapshotJsonCompatibilityTests
{
    [Fact]
    public async Task GoldenFiles_WhenRegenerated_MatchExactly()
    {
        string root = RepositoryRoot();
        string golden = Path.Combine(root, "tests", "RedisKeyMap.Reporting.Tests", "Golden");
        var snapshot = await new SnapshotJsonReader().ReadAsync(Path.Combine(golden, "expected-snapshot.json"), TestContext.Current.CancellationToken);
        string temporary = Path.GetTempFileName();
        try
        {
            await new SnapshotJsonWriter().WriteAsync(snapshot, temporary, TestContext.Current.CancellationToken);
            Assert.Equal(Normalize(await File.ReadAllTextAsync(Path.Combine(golden, "expected-snapshot.json"), TestContext.Current.CancellationToken)), Normalize(await File.ReadAllTextAsync(temporary, TestContext.Current.CancellationToken)));
            Assert.Equal(Normalize(await File.ReadAllTextAsync(Path.Combine(golden, "expected-report.md"), TestContext.Current.CancellationToken)), Normalize(new MarkdownReportWriter().Render(snapshot)));

            var baseline = await new SnapshotJsonReader().ReadAsync(Path.Combine(root, "examples", "baseline-snapshot.json"), TestContext.Current.CancellationToken);
            var current = await new SnapshotJsonReader().ReadAsync(Path.Combine(root, "examples", "current-snapshot.json"), TestContext.Current.CancellationToken);
            string diff = new MarkdownDiffReportWriter().Render(new SnapshotComparer().Compare(baseline, current));
            Assert.Equal(Normalize(await File.ReadAllTextAsync(Path.Combine(golden, "expected-diff.md"), TestContext.Current.CancellationToken)), Normalize(diff));
        }
        finally
        {
            File.Delete(temporary);
        }
    }

    private static string Normalize(string value) => value.Replace("\r\n", "\n", StringComparison.Ordinal);
    private static string RepositoryRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null && !File.Exists(Path.Combine(current.FullName, "RedisKeyMap.slnx")))
        {
            current = current.Parent;
        }

        return current?.FullName ?? throw new InvalidOperationException("Repository root not found.");
    }
}
