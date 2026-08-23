using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace RedisKeyMap.Core.Normalization;

public sealed record CompiledNormalizationRule(CustomNormalizationRule Definition, Regex Regex, bool UsesBacktracking);

public static partial class NormalizationRuleCompiler
{
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(100);

    public static ImmutableArray<CompiledNormalizationRule> Compile(IEnumerable<CustomNormalizationRule> rules)
    {
        HashSet<string> names = new(StringComparer.Ordinal);
        ImmutableArray<CompiledNormalizationRule>.Builder result = ImmutableArray.CreateBuilder<CompiledNormalizationRule>();
        foreach (CustomNormalizationRule rule in rules)
        {
            if (string.IsNullOrWhiteSpace(rule.Name) || !names.Add(rule.Name))
            {
                throw new ArgumentException($"Duplicate or empty custom rule name: '{rule.Name}'.");
            }

            if (string.IsNullOrEmpty(rule.Pattern))
            {
                throw new ArgumentException($"Custom rule '{rule.Name}' has an empty pattern.");
            }

            if (!MarkerRegex().IsMatch(rule.Replacement))
            {
                throw new ArgumentException($"Custom rule '{rule.Name}' has invalid replacement marker '{rule.Replacement}'.");
            }

            if (SegmentMarkers.BuiltIns.Contains(rule.Replacement, StringComparer.Ordinal))
            {
                throw new ArgumentException($"Custom rule '{rule.Name}' cannot redefine built-in marker '{rule.Replacement}'.");
            }

            RegexOptions options = RegexOptions.CultureInvariant | (rule.IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None);
            try
            {
                result.Add(new(rule, new Regex(rule.Pattern, options | RegexOptions.NonBacktracking, MatchTimeout), false));
            }
            catch (NotSupportedException)
            {
                result.Add(new(rule, new Regex(rule.Pattern, options, MatchTimeout), true));
            }
            catch (ArgumentException exception)
            {
                throw new ArgumentException($"Custom rule '{rule.Name}' has an invalid regular expression.", exception);
            }
        }
        return result.ToImmutable();
    }

    [GeneratedRegex("""^\{[a-z][a-z0-9-]*\}$""", RegexOptions.CultureInvariant)]
    private static partial Regex MarkerRegex();
}
