using System.Text;
using BenchmarkDotNet.Attributes;
using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Normalization;

namespace RedisKeyMap.Benchmarks;

[MemoryDiagnoser]
public class NormalizationBenchmarks
{
    private readonly KeyNormalizer _builtIn = new(new());
    private readonly KeyNormalizer _custom = new(new()
    {
        CustomRules =
        [
            new("tenant", "^tenant-[0-9]+$", "{tenant-id}"),
            new("region", "^region-[a-z]+$", "{region}"),
            new("shard", "^shard-[0-9]+$", "{shard}"),
            new("date", "^[0-9]{4}-[0-9]{2}-[0-9]{2}$", "{date}"),
            new("opaque", "^opaque-[A-Za-z0-9]+$", "{opaque}")
        ]
    });
    private readonly KeyObservation _observation = Create("user:550e8400-e29b-41d4-a716-446655440000:orders");

    [Benchmark(Baseline = true)]
    public KeyPattern BuiltIn() => _builtIn.Normalize(_observation);

    [Benchmark]
    public KeyPattern FiveCustomRules() => _custom.Normalize(_observation);

    private static KeyObservation Create(string value) => new(Encoding.UTF8.GetBytes(value), value, "benchmark");
}
