using System.Collections.Immutable;
using System.Security.Cryptography;
using RedisKeyMap.Core.Findings;
using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Normalization;
using RedisKeyMap.Core.Privacy;
using RedisKeyMap.Core.Utilities;

namespace RedisKeyMap.Core.Aggregation;

public sealed class KeyspaceAnalyzer
{
    public async Task<AnalysisResult> AnalyzeAsync(
        IAsyncEnumerable<KeyObservation> source,
        AnalysisOptions options,
        SnapshotSource sourceMetadata,
        TimeProvider timeProvider,
        string toolVersion,
        CancellationToken cancellationToken)
    {
        Validate(options);
        DateTimeOffset started = timeProvider.GetUtcNow();
        KeyNormalizer normalizer = new(options.Normalization);
        ExampleMasker masker = new(options.Privacy, options.Normalization.Delimiter);
        Dictionary<string, PatternAccumulator> patterns = new(StringComparer.Ordinal);
        Dictionary<string, long> namespaces = new(StringComparer.Ordinal);
        PatternTrie technicalTree = new();
        PatternTrie logicalTree = new();
        HashSet<ulong>? hashes = options.DuplicateHandling == DuplicateHandling.Hash64 ? [] : null;
        HashSet<byte[]>? exact = options.DuplicateHandling == DuplicateHandling.Exact ? new(ByteArrayComparer.Instance) : null;
        HashSet<string> endpoints = new(StringComparer.Ordinal);
        long observed = 0;
        long accepted = 0;
        long duplicates = 0;
        int maximumDepth = 0;
        bool limited = false;

        await foreach (KeyObservation observation in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            observed++;
            if (!Accept(observation.RawBytes, hashes, exact))
            {
                duplicates++;
                continue;
            }
            if (options.MaxKeys is long max && accepted >= max)
            {
                limited = true;
                break;
            }

            accepted++;
            endpoints.Add(Endpoint(observation.SourceEndpoint, options.Privacy.HideEndpoints));
            KeyPattern pattern = normalizer.Normalize(observation);
            string top = pattern.Segments[0].Value.Length == 0 ? "(empty)" : pattern.Segments[0].Value;
            namespaces[top] = namespaces.GetValueOrDefault(top) + 1;
            if (!patterns.TryGetValue(pattern.Pattern, out PatternAccumulator? accumulator))
            {
                accumulator = new(pattern);
                patterns.Add(pattern.Pattern, accumulator);
            }
            accumulator.Add(masker.Mask(observation, pattern), options.Privacy.ExamplesPerPattern);
            technicalTree.Insert(pattern.Segments.Select(segment => segment.Value));
            logicalTree.Insert(pattern.LogicalSegments);
            maximumDepth = Math.Max(maximumDepth, pattern.Segments.Length);
        }

        DateTimeOffset completed = timeProvider.GetUtcNow();
        ImmutableArray<PatternStats> patternStats = StableOrdering.Patterns(patterns.Values.Select(value => value.ToStats())).ToImmutableArray();
        ImmutableArray<NamespaceStats> namespaceStats = StableOrdering.Namespaces(namespaces.Select(item => new NamespaceStats(item.Key, item.Value))).ToImmutableArray();
        ScanMetadata scan = new(
            started,
            completed,
            Math.Max(0, (long)(completed - started).TotalMilliseconds),
            observed,
            accepted,
            duplicates,
            0,
            limited,
            false,
            !limited,
            options.DuplicateHandling,
            [.. endpoints.Order(StringComparer.Ordinal)],
            options.DuplicateHandling == DuplicateHandling.None ? ["Duplicate filtering is disabled; counts represent observations."] : []);
        ImmutableArray<TreeSnapshotNode> technical = technicalTree.ToSnapshot();
        ImmutableArray<Finding> findings = FindingEngine.Evaluate(accepted, maximumDepth, namespaceStats, patternStats, technical, scan, options);
        List<string> recommendations = [.. BuiltInFindingRules.DefaultRecommendations];
        foreach (Finding finding in findings)
        {
            string? recommendation = BuiltInFindingRules.Recommendation(finding.RuleId);
            if (recommendation is not null && !recommendations.Contains(recommendation, StringComparer.Ordinal))
            {
                recommendations.Add(recommendation);
            }
        }
        Snapshot snapshot = new(
            1,
            toolVersion,
            completed,
            CanonicalConfigHasher.Compute(options.Normalization),
            sourceMetadata with { ContainsRawExamples = options.Privacy.ExampleMode == ExampleMode.Raw },
            scan,
            accepted,
            patternStats.Length,
            namespaceStats.Length,
            maximumDepth,
            namespaceStats,
            patternStats,
            technical,
            logicalTree.ToSnapshot(),
            findings,
            [.. recommendations]);
        return new(snapshot);
    }

    private static bool Accept(ReadOnlyMemory<byte> bytes, HashSet<ulong>? hashes, HashSet<byte[]>? exact)
    {
        if (hashes is not null)
        {
            return hashes.Add(KeyFingerprint.Compute64(bytes.Span));
        }

        if (exact is not null)
        {
            return exact.Add(bytes.ToArray());
        }

        return true;
    }

    private static string Endpoint(string endpoint, bool hide)
    {
        if (!hide)
        {
            return endpoint;
        }

        byte[] hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(endpoint));
        return $"sha256:{Convert.ToHexString(hash.AsSpan(0, 6)).ToLowerInvariant()}";
    }

    private static void Validate(AnalysisOptions options)
    {
        if (options.MaxKeys <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Max keys must be greater than zero.");
        }

        if (options.Privacy.ExamplesPerPattern is < 0 or > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Examples per pattern must be between 0 and 20.");
        }
    }

    private sealed class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public static ByteArrayComparer Instance { get; } = new();
        public bool Equals(byte[]? x, byte[]? y) => ReferenceEquals(x, y) || (x is not null && y is not null && x.AsSpan().SequenceEqual(y));
        public int GetHashCode(byte[] value) => unchecked((int)KeyFingerprint.Compute64(value));
    }
}
