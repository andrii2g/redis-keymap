using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Application.Abstractions;

public interface IDiffReportWriter
{
    Task WriteAsync(DiffResult result, PolicyEvaluation? policy, string path, CancellationToken cancellationToken);
}
