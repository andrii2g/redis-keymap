using RedisKeyMap.Reporting.Markdown;

namespace RedisKeyMap.Reporting.Tests;

public sealed class MarkdownReportWriterTests
{
    [Fact]
    public void Render_WhenSnapshotProvided_IncludesAllRequiredHeadingsAndEscapedPattern()
    {
        string result = new MarkdownReportWriter().Render(TestSnapshots.Create());
        string[] headings =
        [
            "# Redis KeyMap Report", "## Summary", "## Scan metadata", "## Top-level namespaces",
            "## Technical key hierarchy", "## Simplified logical hierarchy", "## Normalized patterns",
            "## Findings", "## Recommendations", "## Method and limitations"
        ];
        foreach (string heading in headings)
        {
            Assert.Contains(heading, result);
        }
    }

    [Fact]
    public void Render_WhenIncompleteAndRaw_IncludesBothWarnings()
    {
        string result = new MarkdownReportWriter().Render(TestSnapshots.Create(false, true));
        Assert.Contains("[!WARNING]", result);
        Assert.Contains("[!CAUTION]", result);
    }
}
