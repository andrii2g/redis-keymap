using System.Collections.Immutable;
using System.Globalization;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Diffing;

public sealed class SnapshotComparer
{
    public DiffResult Compare(Snapshot oldSnapshot, Snapshot newSnapshot, bool allowConfigMismatch = false, bool allowSourceMismatch = false, decimal countChangeThreshold = 0, int maximumDepth = 6)
    {
        SnapshotCompatibility compatibility = SnapshotCompatibilityValidator.Validate(oldSnapshot, newSnapshot, allowConfigMismatch, allowSourceMismatch);
        if (!compatibility.IsCompatible)
        {
            return new(false, compatibility.Messages, Describe(oldSnapshot), Describe(newSnapshot), [], new(0, 0, 0));
        }

        List<DiffChange> changes = [];
        if (compatibility.ConfigurationMismatch)
        {
            changes.Add(new(ChangeKind.ConfigurationChanged, FindingSeverity.Warning, "normalization", oldSnapshot.ConfigurationFingerprint, newSnapshot.ConfigurationFingerprint, "Normalization configuration changed."));
        }
        CompareCounts(oldSnapshot.Patterns.ToDictionary(item => item.Pattern, item => item.Count, StringComparer.Ordinal),
            newSnapshot.Patterns.ToDictionary(item => item.Pattern, item => item.Count, StringComparer.Ordinal),
            ChangeKind.PatternAdded, ChangeKind.PatternRemoved, ChangeKind.PatternCountChanged, compatibility.ConfigurationMismatch);
        CompareCounts(oldSnapshot.Namespaces.ToDictionary(item => item.Name, item => item.Count, StringComparer.Ordinal),
            newSnapshot.Namespaces.ToDictionary(item => item.Name, item => item.Count, StringComparer.Ordinal),
            ChangeKind.NamespaceAdded, ChangeKind.NamespaceRemoved, ChangeKind.NamespaceCountChanged, false);

        Dictionary<(string, string?), Finding> oldFindings = oldSnapshot.Findings.ToDictionary(item => (item.RuleId, item.Pattern));
        Dictionary<(string, string?), Finding> newFindings = newSnapshot.Findings.ToDictionary(item => (item.RuleId, item.Pattern));
        foreach (var item in newFindings.Keys.Except(oldFindings.Keys).OrderBy(key => key.Item1, StringComparer.Ordinal).ThenBy(key => key.Item2, StringComparer.Ordinal))
        {
            Finding finding = newFindings[item];
            changes.Add(new(ChangeKind.FindingIntroduced, finding.Severity, Subject(item), null, finding.Message, "Finding introduced."));
        }
        foreach (var item in oldFindings.Keys.Except(newFindings.Keys).OrderBy(key => key.Item1, StringComparer.Ordinal).ThenBy(key => key.Item2, StringComparer.Ordinal))
        {
            changes.Add(new(ChangeKind.FindingResolved, FindingSeverity.Info, Subject(item), oldFindings[item].Message, null, "Finding resolved."));
        }
        if (oldSnapshot.MaximumDepth != newSnapshot.MaximumDepth)
        {
            FindingSeverity severity = oldSnapshot.MaximumDepth <= maximumDepth && newSnapshot.MaximumDepth > maximumDepth ? FindingSeverity.Warning : FindingSeverity.Info;
            changes.Add(new(ChangeKind.MaximumDepthChanged, severity, "maximumDepth", oldSnapshot.MaximumDepth.ToString(CultureInfo.InvariantCulture), newSnapshot.MaximumDepth.ToString(CultureInfo.InvariantCulture), "Maximum key depth changed."));
        }
        if (oldSnapshot.Scan.IsComplete != newSnapshot.Scan.IsComplete)
        {
            changes.Add(new(ChangeKind.ScanCompletenessChanged, FindingSeverity.Warning, "scanCompleteness", oldSnapshot.Scan.IsComplete.ToString(), newSnapshot.Scan.IsComplete.ToString(), "Scan completeness changed."));
        }

        ImmutableArray<DiffChange> ordered = [.. changes.OrderByDescending(item => item.Severity).ThenBy(item => item.Kind).ThenBy(item => item.Subject, StringComparer.Ordinal)];
        return new(true, compatibility.Messages, Describe(oldSnapshot), Describe(newSnapshot), ordered,
            new(ordered.Count(item => item.Severity == FindingSeverity.Info), ordered.Count(item => item.Severity == FindingSeverity.Warning), ordered.Count(item => item.Severity == FindingSeverity.Error)));

        void CompareCounts(Dictionary<string, long> oldValues, Dictionary<string, long> newValues, ChangeKind added, ChangeKind removed, ChangeKind changed, bool downgrade)
        {
            foreach (string subject in newValues.Keys.Except(oldValues.Keys, StringComparer.Ordinal).Order(StringComparer.Ordinal))
            {
                changes.Add(new(added, FindingSeverity.Info, subject, null, newValues[subject].ToString(CultureInfo.InvariantCulture), "Subject was added."));
            }
            foreach (string subject in oldValues.Keys.Except(newValues.Keys, StringComparer.Ordinal).Order(StringComparer.Ordinal))
            {
                changes.Add(new(removed, downgrade ? FindingSeverity.Info : FindingSeverity.Warning, subject, oldValues[subject].ToString(CultureInfo.InvariantCulture), null, "Subject was removed."));
            }
            foreach (string subject in oldValues.Keys.Intersect(newValues.Keys, StringComparer.Ordinal).Order(StringComparer.Ordinal))
            {
                long oldCount = oldValues[subject];
                long newCount = newValues[subject];
                if (oldCount == newCount)
                {
                    continue;
                }

                decimal percent = oldCount == 0 ? 100 : (newCount - oldCount) * 100m / oldCount;
                if (Math.Abs(percent) < countChangeThreshold)
                {
                    continue;
                }

                changes.Add(new(changed, FindingSeverity.Info, subject, oldCount.ToString(CultureInfo.InvariantCulture), newCount.ToString(CultureInfo.InvariantCulture), $"Count changed by {percent:F1}%."));
            }
        }
    }

    private static string Describe(Snapshot snapshot) => snapshot.Source.SourceLabel ?? snapshot.GeneratedAtUtc.ToString("O", CultureInfo.InvariantCulture);
    private static string Subject((string, string?) identity) => identity.Item2 is null ? identity.Item1 : $"{identity.Item1}:{identity.Item2}";
}
