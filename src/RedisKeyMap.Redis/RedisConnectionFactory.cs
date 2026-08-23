using StackExchange.Redis;

namespace RedisKeyMap.Redis;

public static class RedisConnectionFactory
{
    public static async Task<ConnectionMultiplexer> ConnectAsync(string connection, CancellationToken cancellationToken)
    {
        ConfigurationOptions options;
        try
        {
            options = ConfigurationOptions.Parse(connection);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to parse Redis connection: {ConnectionStringSanitizer.Sanitize(exception.Message, connection)}");
        }
        options.AbortOnConnectFail = false;
        options.AllowAdmin = false;
        options.ConnectRetry = 1;
        try
        {
            return await ConnectionMultiplexer.ConnectAsync(options).WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Failed to connect to Redis: {ConnectionStringSanitizer.Sanitize(exception.Message, connection)}");
        }
    }
}
