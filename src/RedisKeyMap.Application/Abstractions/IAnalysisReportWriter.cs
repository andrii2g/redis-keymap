using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Application.Abstractions;

public interface IAnalysisReportWriter
{
    Task WriteAsync(Snapshot snapshot, string path, CancellationToken cancellationToken);
}
