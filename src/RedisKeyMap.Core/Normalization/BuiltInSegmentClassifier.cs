using System.Text.RegularExpressions;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Normalization;

public static partial class BuiltInSegmentClassifier
{
    public static NormalizedSegment Classify(string segment)
    {
        if (segment.Length == 0)
        {
            return new(segment, segment, SegmentKind.Empty, NormalizationConfidence.Certain);
        }

        if (NumericRegex().IsMatch(segment))
        {
            return new(segment, SegmentMarkers.NumericId, SegmentKind.NumericId, NormalizationConfidence.Heuristic);
        }

        if (UuidRegex().IsMatch(segment))
        {
            return new(segment, SegmentMarkers.Uuid, SegmentKind.Uuid, NormalizationConfidence.Certain);
        }

        if (UlidRegex().IsMatch(segment))
        {
            return new(segment, SegmentMarkers.Ulid, SegmentKind.Ulid, NormalizationConfidence.Certain);
        }

        if (HexRegex().IsMatch(segment))
        {
            return new(segment, SegmentMarkers.Hex, SegmentKind.Hex, NormalizationConfidence.Heuristic);
        }

        if (TokenRegex().IsMatch(segment) && HasAsciiLetterAndDigit(segment))
        {
            return new(segment, SegmentMarkers.Token, SegmentKind.Token, NormalizationConfidence.Heuristic);
        }

        return new(segment, segment, SegmentKind.Static, NormalizationConfidence.Certain);
    }

    private static bool HasAsciiLetterAndDigit(string value)
    {
        bool letter = false;
        bool digit = false;
        foreach (char character in value)
        {
            letter |= character is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
            digit |= character is >= '0' and <= '9';
        }
        return letter && digit;
    }

    [GeneratedRegex("^[0-9]+$", RegexOptions.CultureInvariant)]
    private static partial Regex NumericRegex();

    [GeneratedRegex("^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$", RegexOptions.CultureInvariant)]
    private static partial Regex UuidRegex();

    [GeneratedRegex("^[0-9A-HJKMNP-TV-Z]{26}$", RegexOptions.CultureInvariant)]
    private static partial Regex UlidRegex();

    [GeneratedRegex("^[0-9A-Fa-f]{12,}$", RegexOptions.CultureInvariant)]
    private static partial Regex HexRegex();

    [GeneratedRegex("^[0-9A-Za-z_-]{16,256}$", RegexOptions.CultureInvariant)]
    private static partial Regex TokenRegex();
}
