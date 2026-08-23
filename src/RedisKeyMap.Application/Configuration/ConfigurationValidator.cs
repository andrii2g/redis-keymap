using System.Collections.Immutable;
using RedisKeyMap.Core.Normalization;

namespace RedisKeyMap.Application.Configuration;

public static class ConfigurationValidator
{
    public static ImmutableArray<ValidationError> Validate(AppConfiguration configuration)
    {
        List<ValidationError> errors = [];
        if (string.IsNullOrEmpty(configuration.Delimiter))
        {
            errors.Add(new("delimiter", "Delimiter cannot be empty."));
        }
        else if (configuration.Delimiter.Length > 16)
        {
            errors.Add(new("delimiter", "Delimiter cannot exceed 16 characters."));
        }

        if (configuration.Privacy.ExamplesPerPattern is < 0 or > 20)
        {
            errors.Add(new("privacy.examplesPerPattern", "Must be between 0 and 20."));
        }

        if (configuration.Analysis.MaximumDepth < 0 || configuration.Analysis.MaximumNamespaces < 0 || configuration.Analysis.MaximumPatterns < 0)
        {
            errors.Add(new("analysis", "Thresholds must be non-negative."));
        }

        if (configuration.Report.MaximumTreeDepth < 0 || configuration.Report.MaximumPatternRows < 0)
        {
            errors.Add(new("report", "Thresholds must be non-negative."));
        }

        if (configuration.Policies.MaximumAllowedDepth < 0 || configuration.Policies.MaximumAllowedNamespaces < 0 || configuration.Policies.MaximumPatternIncreasePercent < 0)
        {
            errors.Add(new("policies", "Thresholds must be non-negative."));
        }

        if (configuration.Policies.AllowedPatterns.Distinct(StringComparer.Ordinal).Count() != configuration.Policies.AllowedPatterns.Length)
        {
            errors.Add(new("policies.allowedPatterns", "Values must be unique."));
        }

        if (configuration.Policies.IgnoredPatterns.Distinct(StringComparer.Ordinal).Count() != configuration.Policies.IgnoredPatterns.Length)
        {
            errors.Add(new("policies.ignoredPatterns", "Values must be unique."));
        }

        try
        {
            NormalizationRuleCompiler.Compile(configuration.Normalization.CustomRules);
        }
        catch (ArgumentException exception)
        {
            errors.Add(new("normalization.customRules", exception.Message));
        }
        return [.. errors];
    }
}
