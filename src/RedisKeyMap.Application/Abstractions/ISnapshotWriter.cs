using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Application.Abstractions;

public interface ISnapshotWriter
{
    Task WriteAsync(Snapshot snapshot, string path, CancellationToken cancellationToken);
}
