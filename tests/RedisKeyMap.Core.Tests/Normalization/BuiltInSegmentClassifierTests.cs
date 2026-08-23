using RedisKeyMap.Core.Models;
using RedisKeyMap.Core.Normalization;

namespace RedisKeyMap.Core.Tests.Normalization;

public sealed class BuiltInSegmentClassifierTests
{
    [Theory]
    [InlineData("123", "{id}", SegmentKind.NumericId)]
    [InlineData("550e8400-e29b-41d4-a716-446655440000", "{uuid}", SegmentKind.Uuid)]
    [InlineData("550E8400-E29B-41D4-A716-446655440000", "{uuid}", SegmentKind.Uuid)]
    [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FAV", "{ulid}", SegmentKind.Ulid)]
    [InlineData("abcdef123456", "{hex}", SegmentKind.Hex)]
    [InlineData("abc123xyz987token", "{token}", SegmentKind.Token)]
    [InlineData("alphabetonlylongword", "alphabetonlylongword", SegmentKind.Static)]
    public void Classify_WhenValueProvided_ReturnsExpected(string input, string value, SegmentKind kind)
    {
        NormalizedSegment result = BuiltInSegmentClassifier.Classify(input);
        Assert.Equal(value, result.Value);
        Assert.Equal(kind, result.Kind);
    }

    [Theory]
    [InlineData("01ARZ3NDEKTSV4RRFFQ69G5FAI")]
    [InlineData("550e8400-e29b-41d4-a716-44665544000z")]
    [InlineData("punctuation!token123")]
    public void Classify_WhenInvalidSpecialForm_DoesNotClaimCertainForm(string input)
    {
        NormalizedSegment result = BuiltInSegmentClassifier.Classify(input);
        Assert.DoesNotContain(result.Kind, new[] { SegmentKind.Uuid, SegmentKind.Ulid });
    }
}
