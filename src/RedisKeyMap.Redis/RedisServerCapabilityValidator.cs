using System.Net;
using StackExchange.Redis;

namespace RedisKeyMap.Redis;

public static class RedisServerCapabilityValidator
{
    public static IServer ResolveStandalone(ConnectionMultiplexer connection)
    {
        EndPoint[] endpoints = connection.GetEndPoints(configuredOnly: true);
        if (endpoints.Length != 1)
        {
            throw new NotSupportedException("Redis KeyMap v1 requires exactly one explicitly configured standalone endpoint.");
        }
        IServer server = connection.GetServer(endpoints[0]);
        if (server.ServerType == ServerType.Cluster)
        {
            throw new NotSupportedException("Redis Cluster scanning is not supported in v1.");
        }
        if (server.ServerType == ServerType.Sentinel)
        {
            throw new NotSupportedException("Redis Sentinel topology discovery is not supported in v1.");
        }
        if (server.ServerType != ServerType.Standalone)
        {
            throw new NotSupportedException($"Redis server type '{server.ServerType}' is not supported in v1.");
        }
        return server;
    }
}
