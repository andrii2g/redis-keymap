using System.Collections.Immutable;
using RedisKeyMap.Core.Models;

namespace RedisKeyMap.Core.Diffing;

public sealed record SnapshotCompatibility(bool IsCompatible, bool ConfigurationMismatch, ImmutableArray<string> Messages);

public static class SnapshotCompatibilityValidator
{
    public static SnapshotCompatibility Validate(Snapshot oldSnapshot, Snapshot newSnapshot, bool allowConfigMismatch, bool allowSourceMismatch)
    {
        List<string> messages = [];
        bool compatible = true;
        bool configMismatch = !string.Equals(oldSnapshot.ConfigurationFingerprint, newSnapshot.ConfigurationFingerprint, StringComparison.Ordinal);
        if (configMismatch)
        {
            messages.Add("Snapshots use different normalization configurations.");
            compatible &= allowConfigMismatch;
        }
        bool sourceMismatch = oldSnapshot.Source.Database != newSnapshot.Source.Database ||
            !string.Equals(oldSnapshot.Source.Match, newSnapshot.Source.Match, StringComparison.Ordinal);
        if (sourceMismatch)
        {
            messages.Add("Snapshots use different source database or match settings.");
            compatible &= allowSourceMismatch;
        }
        if (!oldSnapshot.Scan.IsComplete || !newSnapshot.Scan.IsComplete)
        {
            messages.Add("One or both snapshots are incomplete; removed-pattern conclusions may be inaccurate.");
        }
        return new(compatible, configMismatch, [.. messages]);
    }
}
