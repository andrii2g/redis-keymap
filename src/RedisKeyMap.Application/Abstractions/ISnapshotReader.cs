using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Application.Abstractions;

public interface ISnapshotReader
{
    Task<Snapshot> ReadAsync(string path, CancellationToken cancellationToken);
}
