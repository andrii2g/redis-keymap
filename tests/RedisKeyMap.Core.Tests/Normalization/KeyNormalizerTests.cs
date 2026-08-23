using System.Collections.Immutable;
using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Normalization;

namespace RedisKeyMap.Core.Tests.Normalization;

public sealed class KeyNormalizerTests
{
    [Fact]
    public void Normalize_WhenConsecutiveAndBoundaryDelimiters_PreservesEmptySegments()
    {
        KeyNormalizer normalizer = new(new() { Delimiter = "::" });
        KeyPattern result = normalizer.Normalize(TestData.Observation("::user::::123::"));
        Assert.Equal("::user::::{id}::", result.Pattern);
        Assert.True(result.ContainsEmptySegment);
    }

    [Fact]
    public void Normalize_WhenCustomRuleMatchesNumeric_CustomRuleWins()
    {
        NormalizationOptions options = new()
        {
            CustomRules = [new("number", "^[0-9]+$", "{tenant-id}")]
        };
        KeyPattern result = new KeyNormalizer(options).Normalize(TestData.Observation("tenant:123"));
        Assert.Equal("tenant:{tenant-id}", result.Pattern);
        Assert.Equal(SegmentKind.Custom, result.Segments[1].Kind);
    }

    [Fact]
    public void Normalize_WhenAllSegmentsCollapse_PreservesFirstMarker()
    {
        KeyPattern result = new KeyNormalizer(new()).Normalize(TestData.Observation("123"));
        Assert.Equal(ImmutableArray.Create("{id}"), result.LogicalSegments);
    }
}
