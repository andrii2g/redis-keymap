namespace RedisKeyMap.Cli.Infrastructure;

public static class ExitCodes
{
    public const int Success = 0;
    public const int OperationalFailure = 1;
    public const int UsageError = 2;
    public const int PolicyViolation = 3;
    public const int PartialResult = 4;
    public const int Cancelled = 130;
}
