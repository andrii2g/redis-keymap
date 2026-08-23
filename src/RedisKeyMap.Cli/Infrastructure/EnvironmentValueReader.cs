namespace RedisKeyMap.Cli.Infrastructure;

public static class EnvironmentValueReader
{
    public static string ReadRequired(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"Environment variable '{name}' is not set or is empty.") : value;
    }

    public static TimeProvider TimeProvider()
    {
        string? value = Environment.GetEnvironmentVariable("SOURCE_DATE_EPOCH");
        return long.TryParse(value, out long seconds)
            ? new FixedTimeProvider(DateTimeOffset.FromUnixTimeSeconds(seconds))
            : System.TimeProvider.System;
    }

    private sealed class FixedTimeProvider(DateTimeOffset value) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => value;
    }
}
