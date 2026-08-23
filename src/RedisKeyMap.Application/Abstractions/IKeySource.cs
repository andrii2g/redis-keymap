using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Application.Abstractions;

public interface IKeySource : IAsyncDisposable
{
    IAsyncEnumerable<KeyObservation> ReadAsync(CancellationToken cancellationToken);
}
