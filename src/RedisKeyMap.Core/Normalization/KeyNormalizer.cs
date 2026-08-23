using System.Collections.Immutable;
using System.Text.RegularExpressions;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Normalization;

public sealed class KeyNormalizer
{
    private readonly NormalizationOptions _options;
    private readonly ImmutableArray<CompiledNormalizationRule> _customRules;
    private readonly HashSet<string> _collapseMarkers;

    public KeyNormalizer(NormalizationOptions options)
    {
        if (string.IsNullOrEmpty(options.Delimiter))
        {
            throw new ArgumentException("Delimiter cannot be empty.", nameof(options));
        }

        if (options.Delimiter.Length > 16)
        {
            throw new ArgumentException("Delimiter cannot exceed 16 characters.", nameof(options));
        }

        _options = options;
        _customRules = NormalizationRuleCompiler.Compile(options.CustomRules);
        _collapseMarkers = new(options.LogicalTree.CollapseMarkers, StringComparer.Ordinal);
    }

    public KeyPattern Normalize(KeyObservation observation)
    {
        if (observation.Text is null)
        {
            NormalizedSegment binary = new(string.Empty, SegmentMarkers.Binary, SegmentKind.Binary, NormalizationConfidence.Certain);
            return new(SegmentMarkers.Binary, [binary], SegmentMarkers.Binary, [SegmentMarkers.Binary], false, true, false);
        }

        string[] originalSegments = observation.Text.Split(_options.Delimiter, StringSplitOptions.None);
        ImmutableArray<NormalizedSegment>.Builder segments = ImmutableArray.CreateBuilder<NormalizedSegment>(originalSegments.Length);
        foreach (string original in originalSegments)
        {
            segments.Add(Classify(original));
        }

        ImmutableArray<NormalizedSegment> normalized = segments.ToImmutable();
        ImmutableArray<string> logical = normalized.Select(segment => segment.Value)
            .Where(value => !_collapseMarkers.Contains(value)).ToImmutableArray();
        if (logical.IsEmpty)
        {
            logical = [normalized[0].Value];
        }

        return new(
            observation.Text,
            normalized,
            string.Join(_options.Delimiter, normalized.Select(segment => segment.Value)),
            logical,
            normalized.Any(segment => segment.Kind == SegmentKind.Empty),
            false,
            normalized.Any(segment => segment.Confidence == NormalizationConfidence.Heuristic));
    }

    private NormalizedSegment Classify(string segment)
    {
        if (segment.Length == 0)
        {
            return BuiltInSegmentClassifier.Classify(segment);
        }

        foreach (CompiledNormalizationRule rule in _customRules)
        {
            try
            {
                if (rule.Regex.IsMatch(segment))
                {
                    return new(segment, rule.Definition.Replacement, SegmentKind.Custom, NormalizationConfidence.Custom);
                }
            }
            catch (RegexMatchTimeoutException)
            {
                throw new InvalidOperationException($"Custom normalization rule '{rule.Definition.Name}' timed out.");
            }
        }
        return BuiltInSegmentClassifier.Classify(segment);
    }
}
