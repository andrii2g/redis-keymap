namespace RedisKeyMap.Cli.Tests;

public sealed class EndToEndCliTests
{
    [Fact]
    public async Task HelpAndVersion_WhenInvoked_ReturnSuccessWithoutAnsi()
    {
        var help = await CliProcessFixture.RunAsync("--help");
        var version = await CliProcessFixture.RunAsync("--version");
        Assert.Equal(0, help.ExitCode);
        Assert.Equal(0, version.ExitCode);
        Assert.Contains("redis-keymap", help.Output);
        Assert.DoesNotContain((char)27, help.Output);
    }

    [Fact]
    public async Task Analyze_WhenSampleProvided_CreatesSnapshotAndReport()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"redis-keymap-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            string root = CliProcessFixture.RepositoryRoot();
            string snapshot = Path.Combine(directory, "snapshot.json");
            string report = Path.Combine(directory, "report.md");
            var result = await CliProcessFixture.RunAsync("analyze", "--input", Path.Combine(root, "examples", "sample-keys.txt"), "--snapshot", snapshot, "--report", report);
            Assert.Equal(0, result.ExitCode);
            Assert.True(File.Exists(snapshot));
            Assert.True(File.Exists(report));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task Check_WhenDeliberateDrift_ReturnsPolicyExitThree()
    {
        string root = CliProcessFixture.RepositoryRoot();
        var result = await CliProcessFixture.RunAsync(
            "check",
            Path.Combine(root, "examples", "baseline-snapshot.json"),
            Path.Combine(root, "examples", "current-snapshot.json"),
            "--config",
            Path.Combine(root, "examples", "redis-keymap.json"));
        Assert.Equal(3, result.ExitCode);
    }
}
