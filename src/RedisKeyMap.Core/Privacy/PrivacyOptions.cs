namespace RedisKeyMap.Core.Privacy;

public enum ExampleMode { Masked, None, Raw }

public sealed record PrivacyOptions
{
    public ExampleMode ExampleMode { get; init; } = ExampleMode.Masked;
    public int ExamplesPerPattern { get; init; } = 3;
    public bool MaskStaticSegments { get; init; }
    public bool HideEndpoints { get; init; }
}
