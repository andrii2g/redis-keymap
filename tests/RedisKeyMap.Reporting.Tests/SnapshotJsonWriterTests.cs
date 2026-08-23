using RedisKeyMap.Reporting.Json;

namespace RedisKeyMap.Reporting.Tests;

public sealed class SnapshotJsonWriterTests
{
    [Fact]
    public async Task WriteAsync_WhenSameSnapshot_WritesDeterministicCamelCaseJson()
    {
        string first = Path.GetTempFileName();
        string second = Path.GetTempFileName();
        try
        {
            SnapshotJsonWriter writer = new();
            await writer.WriteAsync(TestSnapshots.Create(), first, TestContext.Current.CancellationToken);
            await writer.WriteAsync(TestSnapshots.Create(), second, TestContext.Current.CancellationToken);
            Assert.Equal(await File.ReadAllTextAsync(first, TestContext.Current.CancellationToken), await File.ReadAllTextAsync(second, TestContext.Current.CancellationToken));
            Assert.Contains("\"schemaVersion\": 1", await File.ReadAllTextAsync(first, TestContext.Current.CancellationToken));
            Assert.Contains("\"textFile\"", await File.ReadAllTextAsync(first, TestContext.Current.CancellationToken));
        }
        finally
        {
            File.Delete(first);
            File.Delete(second);
        }
    }

    [Fact]
    public async Task ReadAsync_WhenRoundTripped_PreservesSnapshot()
    {
        string path = Path.GetTempFileName();
        try
        {
            await new SnapshotJsonWriter().WriteAsync(TestSnapshots.Create(), path, TestContext.Current.CancellationToken);
            var result = await new SnapshotJsonReader().ReadAsync(path, TestContext.Current.CancellationToken);
            Assert.Equal(2, result.TotalKeys);
            Assert.Equal("user:{id}", Assert.Single(result.Patterns).Pattern);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
