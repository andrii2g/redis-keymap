using RedisKeyMap.Core.Models;
using RedisKeyMap.Reporting.Markdown;

namespace RedisKeyMap.Reporting.Tests;

public sealed class MarkdownDiffReportWriterTests
{
    [Fact]
    public void Render_WhenNoChanges_StatesNoneForEveryCategory()
    {
        DiffResult result = new(true, [], "old", "new", [], new(0, 0, 0));
        string markdown = new MarkdownDiffReportWriter().Render(result);
        Assert.Contains("# Redis KeyMap Drift Report", markdown);
        Assert.Contains("None.", markdown);
    }
}
