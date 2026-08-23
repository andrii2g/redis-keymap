using StackExchange.Redis;
using Testcontainers.Redis;

namespace RedisKeyMap.Redis.Tests;

public sealed class RedisKeySourceIntegrationTests
{
    [Fact]
    public async Task ReadAsync_WhenStandaloneRedis_UsesCursorScanAndReturnsUnicodeAndBinaryKeys()
    {
        RedisContainer container;
        try
        {
            container = new RedisBuilder("redis:7.4-alpine").Build();
        }
        catch (DotNet.Testcontainers.Builders.DockerUnavailableException)
        {
            Assert.Skip("Docker is unavailable; the Redis integration test is required and runs in CI.");
            return;
        }
        await using RedisContainer disposableContainer = container;
        await container.StartAsync(TestContext.Current.CancellationToken);
        string connectionString = container.GetConnectionString();
        await using (ConnectionMultiplexer connection = await ConnectionMultiplexer.ConnectAsync(connectionString))
        {
            IDatabase database = connection.GetDatabase();
            await database.StringSetAsync("user:1", "test");
            await database.StringSetAsync("ключ:2", "test");
            await database.StringSetAsync((RedisKey)new byte[] { 0, 255, 1 }, "test");
        }

        await using RedisKeySource source = await RedisKeySource.ConnectAsync(connectionString, new(PageSize: 1), TestContext.Current.CancellationToken);
        List<RedisKeyMap.Core.Models.KeyObservation> observations = [];
        await foreach (var observation in source.ReadAsync(TestContext.Current.CancellationToken))
        {
            observations.Add(observation);
        }

        Assert.Equal(3, observations.Count);
        Assert.Contains(observations, item => item.Text == "user:1");
        Assert.Contains(observations, item => item.Text == "ключ:2");
        Assert.Contains(observations, item => item.Text is null);
    }
}
