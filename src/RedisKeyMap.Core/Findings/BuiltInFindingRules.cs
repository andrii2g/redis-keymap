namespace RedisKeyMap.Core.Findings;

public static class BuiltInFindingRules
{
    public static readonly string[] DefaultRecommendations =
    [
        "Document the principal Redis key patterns in the owning application repository.",
        "Prefer a consistent key form such as entity:{id}:resource.",
        "Review this snapshot before Redis cleanup or naming refactors."
    ];

    public static string? Recommendation(string ruleId) => ruleId switch
    {
        "RKM001" => "Review keys with empty segments and standardize delimiter usage.",
        "RKM002" => "Reduce deeply nested key names where practical.",
        "RKM006" => "Standardize namespace and segment casing.",
        "RKM007" => "Document binary-key producers separately.",
        "RKM008" => "Repeat analysis with a complete scan before drawing removal conclusions.",
        "RKM010" => "Add custom normalization rules for unrecognized high-cardinality segments.",
        _ => null
    };
}
