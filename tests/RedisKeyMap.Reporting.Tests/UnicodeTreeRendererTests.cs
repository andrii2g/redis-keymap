using RedisKeyMap.Reporting.Markdown;

namespace RedisKeyMap.Reporting.Tests;

public sealed class UnicodeTreeRendererTests
{
    [Fact]
    public void Render_WhenTreeProvided_UsesUnicodeAndCounts()
    {
        string result = new UnicodeTreeRenderer().Render(TestSnapshots.Create().TechnicalTree);
        Assert.Contains("user  (2)", result);
        Assert.Contains("└─ {id}  (2)", result);
    }
}
