namespace RedisKeyMap.Cli.Tests;

public sealed class CommandValidationTests
{
    [Fact]
    public async Task Scan_WhenBothConnectionSourcesMissing_ReturnsUsageError()
    {
        var result = await CliProcessFixture.RunAsync("scan");
        Assert.Equal(2, result.ExitCode);
        Assert.Contains("provide exactly one Redis connection source", result.Error);
    }

    [Fact]
    public async Task Analyze_WhenInputMissing_ReturnsUsageError()
    {
        var result = await CliProcessFixture.RunAsync("analyze");
        Assert.Equal(2, result.ExitCode);
    }
}
