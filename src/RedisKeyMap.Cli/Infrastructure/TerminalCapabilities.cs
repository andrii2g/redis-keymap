namespace RedisKeyMap.Cli.Infrastructure;

public static class TerminalCapabilities
{
    public static bool SupportsAnsi(bool noColor) => !noColor && !Console.IsOutputRedirected;
}
