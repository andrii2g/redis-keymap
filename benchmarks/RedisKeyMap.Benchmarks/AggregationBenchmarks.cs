using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using RedisKeyMap.Core.Aggregation;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Benchmarks;

[MemoryDiagnoser]
public class AggregationBenchmarks
{
    [Params(100_000, 1_000_000)]
    public int Count { get; set; }

    [Benchmark]
    public async Task<long> Analyze()
    {
        AnalysisResult result = await new KeyspaceAnalyzer().AnalyzeAsync(
            Observations(Count),
            new() { DuplicateHandling = DuplicateHandling.Hash64 },
            new SnapshotSource(SourceKind.TextFile),
            TimeProvider.System,
            "benchmark",
            CancellationToken.None);
        return result.Snapshot.TotalKeys;
    }

    private static async IAsyncEnumerable<KeyObservation> Observations(int count, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (int index = 0; index < count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string value = $"entity:{index}:resource:{index % 1000}";
            yield return new(Encoding.UTF8.GetBytes(value), value, "benchmark");
        }
        await Task.CompletedTask;
    }
}
