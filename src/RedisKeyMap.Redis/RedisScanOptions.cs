namespace RedisKeyMap.Redis;

public sealed record RedisScanOptions(int Database = 0, string Match = "*", int PageSize = 1000);
