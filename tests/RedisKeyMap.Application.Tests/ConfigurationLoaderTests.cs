using RedisKeyMap.Application.Configuration;

namespace RedisKeyMap.Application.Tests;

public sealed class ConfigurationLoaderTests
{
    [Fact]
    public async Task LoadAsync_WhenNoPath_ReturnsValidatedDefaults()
    {
        AppConfiguration result = await new ConfigurationLoader().LoadAsync(null, TestContext.Current.CancellationToken);
        Assert.Equal(":", result.Delimiter);
        Assert.Equal(3, result.Privacy.ExamplesPerPattern);
    }

    [Fact]
    public async Task LoadAsync_WhenUnknownProperty_RejectsConfiguration()
    {
        string path = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(path, "{ \"unknown\": true }", TestContext.Current.CancellationToken);
            await Assert.ThrowsAsync<InvalidDataException>(() => new ConfigurationLoader().LoadAsync(path, TestContext.Current.CancellationToken));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
